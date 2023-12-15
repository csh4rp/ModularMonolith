using System.Data;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using Npgsql;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal sealed class EventReader : IEventReader
{
    private readonly EventMetaDataProvider _eventMetaDataProvider;
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly IOptionsMonitor<EventOptions> _optionsMonitor;

    public EventReader(DbConnectionFactory dbConnectionFactory, 
        EventMetaDataProvider eventMetaDataProvider,
        IOptionsMonitor<EventOptions> optionsMonitor)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _eventMetaDataProvider = eventMetaDataProvider;
        _optionsMonitor = optionsMonitor;
    }

    public async Task<(bool WasAcquired, EventLog? EventLog)> TryAcquireLockAsync(EventInfo eventInfo, CancellationToken cancellationToken)
    {
        var retryAttempts = _optionsMonitor.CurrentValue.MaxRetryAttempts;
        var (id, correlationId) = eventInfo;
        
        await using var connection = _dbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        
        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
        var eventLockMetaData = _eventMetaDataProvider.EventLogLockMetaData;
        var eventCorrelationLockMetaData = _eventMetaDataProvider.EventLogCorrelationLockMetaData;

        await using var batch = connection.CreateBatch();
        
        if (correlationId.HasValue)
        {
            var insertCommand = batch.CreateBatchCommand();
            insertCommand.Parameters.AddWithValue("@correlation_id", correlationId.Value);
            insertCommand.CommandText = 
                $"""
                 INSERT INTO {eventCorrelationLockMetaData.TableName}
                 ({eventCorrelationLockMetaData.CorrelationIdColumnName}, {eventCorrelationLockMetaData.AcquiredAtColumnName})
                 VALUES (@correlation_id, CURRENT_TIMESTAMP)
                 ON CONFLICT DO NOTHING
                 RETURNING 1;
                 """;

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
                    FROM {eventLogMetaData.TableName}
                    WHERE {eventLogMetaData.CorrelationIdColumnName} = el.{eventLogMetaData.CorrelationIdColumnName}
                    AND {eventLogMetaData.CreatedAtColumnName} < el.{eventLogMetaData.CreatedAtColumnName}
                    AND {eventLogMetaData.PublishedAtColumnName} IS NULL
                    AND {eventLogMetaData.AttemptNumberColumnName} < @max_retry_attempts
                )
                AND ({eventLogMetaData.NextAttemptAtColumnName} IS NULL OR {eventLogMetaData.NextAttemptAtColumnName} < CURRENT_TIMESTAMP)
                AND {eventLogMetaData.AttemptNumberColumnName} < @max_retry_attempts
                ORDER BY {eventLogMetaData.CreatedAtColumnName}
                LIMIT 1 OFFSET 0 
                """;
        }
        else
        {
            var insertCommand = batch.CreateBatchCommand();
            insertCommand.Parameters.AddWithValue("@id", id);
            insertCommand.Parameters.AddWithValue("@max_retry_attempts", retryAttempts);
            insertCommand.CommandText = 
                $"""
                 INSERT INTO {eventLockMetaData.TableName}
                 ({eventLockMetaData.IdColumnName}, {eventLockMetaData.AcquiredAtColumnName})
                 VALUES (@id, CURRENT_TIMESTAMP)
                 ON CONFLICT DO NOTHING
                 RETURNING 1;
                 """;
            
            var selectCommand = batch.CreateBatchCommand();
            selectCommand.Parameters.AddWithValue("@id", id);
            selectCommand.CommandText =
                $"""
                 SELECT *
                 FROM {eventLogMetaData.TableName}
                 WHERE {eventLogMetaData.PublishedAtColumnName} IS NULL
                 AND {eventLogMetaData.IdColumnName} = @id
                 AND NOT EXISTS
                 (
                     SELECT 1
                     FROM {eventLockMetaData.TableName}
                     WHERE {eventLockMetaData.IdColumnName} = @id
                 )
                 AND ({eventLogMetaData.NextAttemptAtColumnName} IS NULL OR {eventLogMetaData.NextAttemptAtColumnName} < CURRENT_TIMESTAMP)
                 AND {eventLogMetaData.AttemptNumberColumnName} < @max_retry_attempts
                 ORDER BY {eventLogMetaData.CreatedAtColumnName}
                 LIMIT 1 OFFSET 0
                 """;
        }
        
        await using var reader = await batch.ExecuteReaderAsync(cancellationToken);

        if (!reader.HasRows || !await reader.ReadAsync(cancellationToken))
        {
            return (false, null);
        }

        _ = await reader.NextResultAsync(cancellationToken);
        
        if (!reader.HasRows || !await reader.ReadAsync(cancellationToken))
        {
            return (false, null);
        }
        
        return (true, ReadEventLog(reader, eventLogMetaData));
    }
    
    private static EventLog ReadEventLog(NpgsqlDataReader reader, EventLogMetaData eventLogMetaData) =>
        new()
        {
            Id = reader.GetGuid(eventLogMetaData.IdColumnName),
            Name = reader.GetString(eventLogMetaData.NameColumnName),
            Payload = reader.GetString(eventLogMetaData.PayloadColumnName),
            Type = reader.GetString(eventLogMetaData.TypeColumnName),
            ActivityId = reader.GetString(eventLogMetaData.ActivityIdColumnName),
            CorrelationId = reader.IsDBNull(eventLogMetaData.CorrelationIdColumnName)
                ? null
                : reader.GetGuid(eventLogMetaData.CorrelationIdColumnName),
            CreatedAt = reader.GetDateTime(eventLogMetaData.CreatedAtColumnName),
            OperationName = reader.GetString(eventLogMetaData.OperationNameColumnName),
            UserId = reader.IsDBNull(eventLogMetaData.UserIdColumnName)
                ? null
                : reader.GetGuid(eventLogMetaData.UserIdColumnName)
        };

    public async Task MarkAsPublishedAsync(EventInfo eventInfo, CancellationToken cancellationToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
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
        }
        
        var markAsPublishedCommand = batch.CreateBatchCommand();
        markAsPublishedCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);
        markAsPublishedCommand.CommandText =
            $"""
             UPDATE {eventLogMetaData.TableName}
             SET {eventLogMetaData.PublishedAtColumnName} = CURRENT_TIMESTAMP
             WHERE {eventLogMetaData.IdColumnName} = @id
             """;

        _ = await batch.ExecuteNonQueryAsync(cancellationToken);
    }
    
    public async Task IncrementFailedAttemptNumberAsync(EventInfo eventInfo, CancellationToken cancellationToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
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
        }
        
        var increaseFailedAttemptCommand = batch.CreateBatchCommand();
        increaseFailedAttemptCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);
        increaseFailedAttemptCommand.CommandText =
            $"""
             UPDATE {eventLogMetaData.TableName}
             SET {eventLogMetaData.AttemptNumberColumnName} = {eventLogMetaData.AttemptNumberColumnName} + 1,
                 {eventLogMetaData.NextAttemptAtColumnName} =
                     COALESCE({eventLogMetaData.NextAttemptAtColumnName}, {eventLogMetaData.CreatedAtColumnName})
                     + INTERVAL '5 minutes' * {eventLogMetaData.AttemptNumberColumnName}
             WHERE {eventLogMetaData.IdColumnName} = @id
             """;

        _ = await batch.ExecuteNonQueryAsync(cancellationToken);
    }
    
    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
        
        await using var connection = _dbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText =
            $"""
             CREATE OR REPLACE FUNCTION notify_event_log_inserted()
                RETURNS TRIGGER
                LANGUAGE PLPGSQL
             AS
             $$
             BEGIN
                NOTIFY new_events, CONCAT(COALESCE(NEW.{eventLogMetaData.CorrelationIdColumnName}::text, ''), '/', NEW.{eventLogMetaData.IdColumnName}::text)
                RETURN NEW;
             END;
             $$

             CREATE OR REPLACE TRIGGER event_inserted
                 AFTER INSERT ON {eventLogMetaData.TableName}
                 FOR EACH ROW
                 EXECUTE FUNCTION notify_event_log_inserted()
             """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventInfo>> GetUnpublishedEventsAsync(CancellationToken cancellationToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.EventLogMetaData;
        var eventLogLockMetaData = _eventMetaDataProvider.EventLogLockMetaData;
        var eventLogCorrelationLockMetaData = _eventMetaDataProvider.EventLogCorrelationLockMetaData;

        await using var connection = _dbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        await using var batch = connection.CreateBatch();

        var releaseEventLocksCommand = batch.CreateBatchCommand();
        releaseEventLocksCommand.Parameters.AddWithValue("@max_lock_time_in_seconds",
            _optionsMonitor.CurrentValue.MaxLockTime.TotalSeconds);

        releaseEventLocksCommand.CommandText =
            $"""
            DELETE FROM {eventLogLockMetaData.TableName}
            WHERE ({eventLogLockMetaData.AcquiredAtColumnName} + INTERVAL '1 second' * @max_lock_time_in_seconds) < CURRENT_TIMESTAMP
            """;

        var releaseCorrelationLocksCommand = batch.CreateBatchCommand();
        releaseEventLocksCommand.Parameters.AddWithValue("@max_lock_time_in_seconds",
            _optionsMonitor.CurrentValue.MaxLockTime.TotalSeconds);

        releaseEventLocksCommand.CommandText =
            $"""
             DELETE FROM {eventLogCorrelationLockMetaData.TableName}
             WHERE ({eventLogCorrelationLockMetaData.AcquiredAtColumnName} + INTERVAL '1 second' * @max_lock_time_in_seconds) < CURRENT_TIMESTAMP
             """;
        
        var selectCommand = batch.CreateBatchCommand();
        selectCommand.Parameters.AddWithValue("@max_retry_attempts", _optionsMonitor.CurrentValue.MaxRetryAttempts);
        selectCommand.CommandText =
            $"""
             SELECT
             {eventLogMetaData.IdColumnName}
             {eventLogMetaData.CorrelationIdColumnName}
             FROM {eventLogMetaData.TableName} AS el
             WHERE {eventLogMetaData.PublishedAtColumnName} IS NULL
             AND 
             NOT EXISTS
             (
                SELECT 1
                FROM {eventLogCorrelationLockMetaData.TableName}
                WHERE {eventLogCorrelationLockMetaData.CorrelationIdColumnName} = el.{eventLogMetaData.CorrelationIdColumnName}
             )
             AND NOT EXISTS
             (
                 SELECT 1
                 FROM {eventLogMetaData.TableName}
                 WHERE {eventLogMetaData.CorrelationIdColumnName} = el.{eventLogMetaData.CorrelationIdColumnName}
                 AND {eventLogMetaData.CreatedAtColumnName} < el.{eventLogMetaData.CreatedAtColumnName}
                 AND {eventLogMetaData.PublishedAtColumnName} IS NULL
                 AND {eventLogMetaData.AttemptNumberColumnName} < @max_retry_attempts
             )
             AND NOT EXISTS
             (
                SELECT 1
                FROM {eventLogLockMetaData.TableName}
                AND {eventLogMetaData.IdColumnName} = el.{eventLogMetaData.IdColumnName}
             )
             AND ({eventLogMetaData.NextAttemptAtColumnName} IS NULL OR {eventLogMetaData.NextAttemptAtColumnName} < CURRENT_TIMESTAMP)
             AND {eventLogMetaData.AttemptNumberColumnName} < @max_retry_attempts
             ORDER BY {eventLogMetaData.CreatedAtColumnName}
             LIMIT 10 OFFSET 0
             """;
        
        await using var reader = await batch.ExecuteReaderAsync(cancellationToken);

        // Move to the SELECT query result
        _ = await reader.NextResultAsync(cancellationToken);
        _ = await reader.NextResultAsync(cancellationToken);
        
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
}
