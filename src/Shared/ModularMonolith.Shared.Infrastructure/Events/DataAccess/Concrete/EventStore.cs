using System.Data;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using Npgsql;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class EventStore : IEventStore
{
    private readonly EventMetaDataProvider _eventMetaDataProvider;
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly IOptionsMonitor<EventOptions> _optionsMonitor;
    private readonly TimeProvider _timeProvider;

    public EventStore(DbConnectionFactory dbConnectionFactory,
        EventMetaDataProvider eventMetaDataProvider,
        IOptionsMonitor<EventOptions> optionsMonitor, TimeProvider timeProvider)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _eventMetaDataProvider = eventMetaDataProvider;
        _optionsMonitor = optionsMonitor;
        _timeProvider = timeProvider;
    }

    public async Task<(bool WasLockAcquired, EventLog? EventLog)> TryAcquireLockAsync(EventInfo eventInfo,
        CancellationToken cancellationToken)
    {
        var retryAttempts = _optionsMonitor.CurrentValue.MaxRetryAttempts;
        var (id, correlationId) = eventInfo;
        var now = _timeProvider.GetUtcNow();

        await using var connection = _dbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
        var eventLockMetaData = _eventMetaDataProvider.EventLogLockMetaData;
        var eventCorrelationLockMetaData = _eventMetaDataProvider.EventLogCorrelationLockMetaData;
        var publishAttemptMetaData = _eventMetaDataProvider.EventLogPublishAttemptMetaData;

        await using var batch = connection.CreateBatch();

        if (correlationId.HasValue)
        {
            var selectCommand = batch.CreateBatchCommand();
            selectCommand.Parameters.AddWithValue("@correlation_id", correlationId.Value);
            selectCommand.Parameters.AddWithValue("@max_retry_attempts", retryAttempts);
            selectCommand.CommandText =
                $"""
                 SELECT *
                 FROM {eventLogMetaData.TableName} AS el
                 WHERE {eventLogMetaData.PublishedAtColumnName} IS NULL
                 AND NOT EXISTS
                 (
                     SELECT 1
                     FROM {eventCorrelationLockMetaData.TableName}
                     WHERE {eventCorrelationLockMetaData.CorrelationIdColumnName} = @correlation_id
                 )
                 AND NOT EXISTS
                 (
                    SELECT 1
                    FROM {eventLockMetaData.TableName}
                    WHERE {eventLogMetaData.IdColumnName} = el.{eventLogMetaData.IdColumnName}
                 )
                 AND
                 (
                     SELECT COALESCE(MAX({publishAttemptMetaData.AttemptNumberColumnName}), 0)
                     FROM {publishAttemptMetaData.TableName}
                     WHERE {publishAttemptMetaData.EventLogIdColumnName} = el.{eventLogMetaData.IdColumnName}
                 ) < @max_retry_attempts
                 AND
                 (
                     SELECT COALESCE(MAX({publishAttemptMetaData.NextAttemptAtColumnName}), CURRENT_TIMESTAMP)
                     FROM {publishAttemptMetaData.TableName}
                     WHERE {publishAttemptMetaData.EventLogIdColumnName} = el.{eventLogMetaData.IdColumnName}
                 ) <= CURRENT_TIMESTAMP
                 ORDER BY {eventLogMetaData.CreatedAtColumnName}
                 LIMIT 1 OFFSET 0
                 """;

            var insertCommand = batch.CreateBatchCommand();
            insertCommand.Parameters.AddWithValue("@correlation_id", correlationId.Value);
            insertCommand.Parameters.AddWithValue("@now", now.UtcDateTime);
            insertCommand.CommandText =
                $"""
                 INSERT INTO {eventCorrelationLockMetaData.TableName}
                 ({eventCorrelationLockMetaData.CorrelationIdColumnName}, {eventCorrelationLockMetaData.AcquiredAtColumnName})
                 VALUES (@correlation_id, @now)
                 ON CONFLICT DO NOTHING
                 RETURNING 1;
                 """;

            batch.BatchCommands.Add(selectCommand);
            batch.BatchCommands.Add(insertCommand);
        }
        else
        {
            var selectCommand = batch.CreateBatchCommand();
            selectCommand.Parameters.AddWithValue("@id", id);
            selectCommand.Parameters.AddWithValue("@max_retry_attempts", retryAttempts);
            selectCommand.Parameters.AddWithValue("@now", now.UtcDateTime);
            selectCommand.CommandText =
                $"""
                 SELECT *
                 FROM {eventLogMetaData.TableName} AS el
                 WHERE {eventLogMetaData.PublishedAtColumnName} IS NULL
                 AND {eventLogMetaData.IdColumnName} = @id
                 AND NOT EXISTS
                 (
                     SELECT 1
                     FROM {eventLockMetaData.TableName}
                     WHERE {eventLockMetaData.IdColumnName} = @id
                 )
                 AND
                 (
                     SELECT COALESCE(MAX({publishAttemptMetaData.AttemptNumberColumnName}), 0)
                     FROM {publishAttemptMetaData.TableName}
                     WHERE {publishAttemptMetaData.EventLogIdColumnName} = el.{eventLogMetaData.IdColumnName}
                 ) < @max_retry_attempts
                 AND
                 (
                     SELECT COALESCE(MAX({publishAttemptMetaData.NextAttemptAtColumnName}), @now)
                     FROM {publishAttemptMetaData.TableName}
                     WHERE {publishAttemptMetaData.EventLogIdColumnName} = el.{eventLogMetaData.IdColumnName}
                 ) <= @now
                 ORDER BY {eventLogMetaData.CreatedAtColumnName}
                 LIMIT 1 OFFSET 0
                 """;

            var insertCommand = batch.CreateBatchCommand();
            insertCommand.Parameters.AddWithValue("@id", id);
            insertCommand.Parameters.AddWithValue("@max_retry_attempts", retryAttempts);
            insertCommand.Parameters.AddWithValue("@now", now.UtcDateTime);
            insertCommand.CommandText =
                $"""
                 INSERT INTO {eventLockMetaData.TableName}
                 ({eventLockMetaData.IdColumnName}, {eventLockMetaData.AcquiredAtColumnName})
                 VALUES (@id, @now)
                 ON CONFLICT DO NOTHING
                 RETURNING 1;
                 """;

            batch.BatchCommands.Add(selectCommand);
            batch.BatchCommands.Add(insertCommand);
        }

        await using var reader = await batch.ExecuteReaderAsync(cancellationToken);

        if (!reader.HasRows || !await reader.ReadAsync(cancellationToken))
        {
            return (false, null);
        }

        var eventLog = ReadEventLog(reader, eventLogMetaData);

        _ = await reader.NextResultAsync(cancellationToken);

        if (!reader.HasRows || !await reader.ReadAsync(cancellationToken))
        {
            return (false, null);
        }

        return (true, eventLog);
    }

    private static EventLog ReadEventLog(NpgsqlDataReader reader, EventLogMetaData eventLogMetaData) =>
        new()
        {
            Id = reader.GetGuid(eventLogMetaData.IdColumnName),
            EventName = reader.GetString(eventLogMetaData.EventNameColumnName),
            EventPayload = reader.GetString(eventLogMetaData.EventPayloadColumnName),
            EventType = reader.GetString(eventLogMetaData.EventTypeColumnName),
            TraceId = reader.GetString(eventLogMetaData.TraceIdColumnName),
            SpanId = reader.GetString(eventLogMetaData.SpanIdColumnName),
            ParentSpanId = reader.IsDBNull(eventLogMetaData.ParentSpanIdColumnName)
                ? null
                : reader.GetString(eventLogMetaData.ParentSpanIdColumnName),
            CorrelationId = reader.IsDBNull(eventLogMetaData.CorrelationIdColumnName)
                ? null
                : reader.GetGuid(eventLogMetaData.CorrelationIdColumnName),
            CreatedAt = new DateTimeOffset(reader.GetDateTime(eventLogMetaData.CreatedAtColumnName), TimeSpan.Zero),
            PublishedAt = null,
            OperationName = reader.GetString(eventLogMetaData.OperationNameColumnName),
            UserId = reader.IsDBNull(eventLogMetaData.UserIdColumnName)
                ? null
                : reader.GetGuid(eventLogMetaData.UserIdColumnName),
            UserName = reader.IsDBNull(eventLogMetaData.UserNameColumnName)
                ? null
                : reader.GetString(eventLogMetaData.UserNameColumnName),
            IpAddress = reader.IsDBNull(eventLogMetaData.IpAddressColumnName)
                ? null
                : reader.GetString(eventLogMetaData.IpAddressColumnName),
            UserAgent = reader.IsDBNull(eventLogMetaData.UserAgentColumnName)
                ? null
                : reader.GetString(eventLogMetaData.UserAgentColumnName)
        };

    public async Task MarkAsPublishedAsync(EventInfo eventInfo, CancellationToken cancellationToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
        var eventLockMetaData = _eventMetaDataProvider.EventLogLockMetaData;
        var eventCorrelationLockMetaData = _eventMetaDataProvider.EventLogCorrelationLockMetaData;
        var eventLogPublishAttemptMetaData = _eventMetaDataProvider.EventLogPublishAttemptMetaData;

        await using var connection = _dbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        await using var batch = connection.CreateBatch();

        if (eventInfo.CorrelationId.HasValue)
        {
            var releaseLockCommand = batch.CreateBatchCommand();
            releaseLockCommand.Parameters.AddWithValue("@correlation_id", eventInfo.CorrelationId.Value);
            releaseLockCommand.CommandText =
                $"""
                 DELETE FROM {eventCorrelationLockMetaData.TableName}
                 WHERE {eventCorrelationLockMetaData.CorrelationIdColumnName} = @correlation_id;
                 """;

            batch.BatchCommands.Add(releaseLockCommand);
        }
        else
        {
            var releaseLockCommand = batch.CreateBatchCommand();
            releaseLockCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);
            releaseLockCommand.CommandText =
                $"""
                 DELETE FROM {eventLockMetaData.TableName}
                 WHERE {eventLockMetaData.IdColumnName} = @id;
                 """;

            batch.BatchCommands.Add(releaseLockCommand);
        }

        var deleteAttemptsCommand = batch.CreateBatchCommand();
        deleteAttemptsCommand.Parameters.AddWithValue("@event_log_id", eventInfo.EventLogId);
        deleteAttemptsCommand.CommandText =
            $"""
             DELETE FROM {eventLogPublishAttemptMetaData.TableName}
             WHERE {eventLogPublishAttemptMetaData.EventLogIdColumnName} = @event_log_id;
             """;

        var markAsPublishedCommand = batch.CreateBatchCommand();
        markAsPublishedCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);
        markAsPublishedCommand.Parameters.AddWithValue("@now", _timeProvider.GetUtcNow().DateTime);
        markAsPublishedCommand.CommandText =
            $"""
             UPDATE {eventLogMetaData.TableName}
             SET {eventLogMetaData.PublishedAtColumnName} = @now
             WHERE {eventLogMetaData.IdColumnName} = @id
             """;

        batch.BatchCommands.Add(deleteAttemptsCommand);
        batch.BatchCommands.Add(markAsPublishedCommand);

        _ = await batch.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddFailedAttemptAsync(EventInfo eventInfo, CancellationToken cancellationToken)
    {
        var eventLogPublishAttemptMetaData = _eventMetaDataProvider.EventLogPublishAttemptMetaData;
        var eventLockMetaData = _eventMetaDataProvider.EventLogLockMetaData;
        var eventCorrelationLockMetaData = _eventMetaDataProvider.EventLogCorrelationLockMetaData;

        await using var connection = _dbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        await using var batch = connection.CreateBatch();

        if (eventInfo.CorrelationId.HasValue)
        {
            var releaseLockCommand = batch.CreateBatchCommand();
            releaseLockCommand.Parameters.AddWithValue("@correlation_id", eventInfo.CorrelationId.Value);
            releaseLockCommand.CommandText =
                $"""
                 DELETE FROM {eventCorrelationLockMetaData.TableName}
                 WHERE {eventCorrelationLockMetaData.CorrelationIdColumnName} = @correlation_id;
                 """;

            batch.BatchCommands.Add(releaseLockCommand);
        }
        else
        {
            var releaseLockCommand = batch.CreateBatchCommand();
            releaseLockCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);
            releaseLockCommand.CommandText =
                $"""
                 DELETE FROM {eventLockMetaData.TableName}
                 WHERE {eventLockMetaData.IdColumnName} = @id;
                 """;

            batch.BatchCommands.Add(releaseLockCommand);
        }

        var nextAttemptAt = _timeProvider.GetUtcNow().Add(_optionsMonitor.CurrentValue.TimeBetweenAttempts).UtcDateTime;

        var insertFailedAttemptCommand = batch.CreateBatchCommand();
        insertFailedAttemptCommand.Parameters.AddWithValue("@event_log_id", eventInfo.EventLogId);
        insertFailedAttemptCommand.Parameters.AddWithValue("@next_attempt_at", nextAttemptAt);
        insertFailedAttemptCommand.CommandText =
            $"""
             INSERT INTO {eventLogPublishAttemptMetaData.TableName}
             ({eventLogPublishAttemptMetaData.EventLogIdColumnName},
              {eventLogPublishAttemptMetaData.NextAttemptAtColumnName},
              {eventLogPublishAttemptMetaData.AttemptNumberColumnName})
              VALUES (@event_log_id, @next_attempt_at,
                     COALESCE
                     (
                         (SELECT MAX({eventLogPublishAttemptMetaData.AttemptNumberColumnName}) + 1
                         FROM {eventLogPublishAttemptMetaData.TableName} AS epa
                         WHERE epa.{eventLogPublishAttemptMetaData.EventLogIdColumnName} = @event_log_id),
                     1))
             """;

        batch.BatchCommands.Add(insertFailedAttemptCommand);

        _ = await batch.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventInfo>> GetUnpublishedEventsAsync(int take, CancellationToken cancellationToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
        var eventLogLockMetaData = _eventMetaDataProvider.EventLogLockMetaData;
        var eventLogCorrelationLockMetaData = _eventMetaDataProvider.EventLogCorrelationLockMetaData;
        var publishAttemptMetaData = _eventMetaDataProvider.EventLogPublishAttemptMetaData;

        var now = _timeProvider.GetUtcNow();
        var maxLockTimeInSeconds = _optionsMonitor.CurrentValue.MaxLockTime.TotalSeconds;
        var releaseLockDate = now.AddSeconds(-1 * maxLockTimeInSeconds);

        await using var connection = _dbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        await using var batch = connection.CreateBatch();

        var releaseEventLocksCommand = batch.CreateBatchCommand();
        releaseEventLocksCommand.Parameters.AddWithValue("@releaseLockDate", releaseLockDate);
        releaseEventLocksCommand.CommandText =
            $"""
             DELETE FROM {eventLogLockMetaData.TableName}
             WHERE {eventLogLockMetaData.AcquiredAtColumnName} < @releaseLockDate
             """;

        var releaseCorrelationLocksCommand = batch.CreateBatchCommand();
        releaseCorrelationLocksCommand.Parameters.AddWithValue("@releaseLockDate", releaseLockDate);
        releaseCorrelationLocksCommand.CommandText =
            $"""
             DELETE FROM {eventLogCorrelationLockMetaData.TableName}
             WHERE {eventLogCorrelationLockMetaData.AcquiredAtColumnName} <= @releaseLockDate
             """;

        var selectCommand = batch.CreateBatchCommand();
        selectCommand.Parameters.AddWithValue("@max_retry_attempts", _optionsMonitor.CurrentValue.MaxRetryAttempts);
        selectCommand.Parameters.AddWithValue("@now", now.UtcDateTime);
        selectCommand.Parameters.AddWithValue("@take", take);
        selectCommand.CommandText =
            $"""
             SELECT
             {eventLogMetaData.IdColumnName},
             {eventLogMetaData.CorrelationIdColumnName}
             FROM {eventLogMetaData.TableName} AS el
             WHERE {eventLogMetaData.PublishedAtColumnName} IS NULL
             AND NOT EXISTS
             (
                SELECT 1
                FROM {eventLogCorrelationLockMetaData.TableName}
                WHERE {eventLogCorrelationLockMetaData.CorrelationIdColumnName} = el.{eventLogMetaData.CorrelationIdColumnName}
             )
             AND NOT EXISTS
             (
                SELECT 1
                FROM {eventLogLockMetaData.TableName}
                WHERE {eventLogMetaData.IdColumnName} = el.{eventLogMetaData.IdColumnName}
             )
             AND
             (
                 SELECT COALESCE(MAX({publishAttemptMetaData.AttemptNumberColumnName}), 0)
                 FROM {publishAttemptMetaData.TableName}
                 WHERE {publishAttemptMetaData.EventLogIdColumnName} = el.{eventLogMetaData.IdColumnName}
             ) < @max_retry_attempts
             AND
             (
                 SELECT COALESCE(MAX({publishAttemptMetaData.NextAttemptAtColumnName}), @now)
                 FROM {publishAttemptMetaData.TableName}
                 WHERE {publishAttemptMetaData.EventLogIdColumnName} = el.{eventLogMetaData.IdColumnName}
             ) <= @now
             ORDER BY {eventLogMetaData.CreatedAtColumnName}
             LIMIT @take OFFSET 0
             """;

        batch.BatchCommands.Add(releaseEventLocksCommand);
        batch.BatchCommands.Add(releaseCorrelationLocksCommand);
        batch.BatchCommands.Add(selectCommand);

        try
        {
            await using var reader = await batch.ExecuteReaderAsync(cancellationToken);

            if (!reader.HasRows)
            {
                return Array.Empty<EventInfo>();
            }

            var events = new List<EventInfo>(10);

            while (await reader.ReadAsync(cancellationToken))
            {
                var eventLogId = reader.GetGuid(0);
                var correlationId = await reader.IsDBNullAsync(1, cancellationToken) ? (Guid?)null : reader.GetGuid(1);

                events.Add(new EventInfo(eventLogId, correlationId));
            }

            return events;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<EventInfo>();
        }
    }
}
