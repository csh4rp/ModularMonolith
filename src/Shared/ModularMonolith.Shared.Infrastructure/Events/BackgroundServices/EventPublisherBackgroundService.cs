using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Npgsql;
using Polly;
using Polly.Retry;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventPublisherBackgroundService : BackgroundService
{
    private static readonly ActivitySource EventPublisherActivitySource = new ActivitySource("");


    
    private readonly EventReader _eventReader;
    private readonly EventSerializer _eventSerializer;
    private readonly EventMapper _eventMapper;
    private readonly ILogger<EventPublisherBackgroundService> _logger;
    private readonly EventChannel _eventChannel;
    private readonly EventMetaDataProvider _eventMetaDataProvider;
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly Task[] _tasks = new Task[Environment.ProcessorCount * 2];



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            _tasks[i] = Task.Factory.StartNew(async () =>
            {
                await foreach (var eventInfo in _eventChannel.ReadAllAsync(stoppingToken))
                {
                    await using var connection = _dbConnectionFactory.Create();
                    await connection.OpenAsync(stoppingToken);

                    await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, stoppingToken);

                    EventLog? eventLog;
                    
                    if (eventInfo.CorrelationId.HasValue)
                    {
                        eventLog = await TryAcquireCorrelationLockAsync(transaction, eventInfo, stoppingToken);
                    }
                    else
                    {
                        eventLog = await GetValueAsync(transaction, stoppingToken);
                    }
                    
                    if (eventLog is null)
                    {
                        continue;
                    }
                    
                    try
                    {
                        await PublishAsync(eventLog, stoppingToken);
                        
                        await MarkAsPublishedAsync(transaction, eventInfo, stoppingToken);
                        
                        await transaction.CommitAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(stoppingToken);
                    }
                    
                }
                
            }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        await Task.WhenAll(_tasks);
        
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Constant,
                MaxRetryAttempts = int.MaxValue
            })
            .Build();

        await pipeline.ExecuteAsync(RunAsync, stoppingToken);
    }

    private async Task MarkAsPublishedAsync(NpgsqlTransaction transaction, EventInfo eventInfo, CancellationToken stoppingToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.GetEventLogMetaData();
        var eventLockMetaData = _eventMetaDataProvider.GetEventLogMetaData();
        
        await using var batch = transaction.Connection!.CreateBatch();

        if (eventInfo.CorrelationId.HasValue)
        {
            var releaseLockCommand = batch.CreateBatchCommand();
            releaseLockCommand.Parameters.AddWithValue("@correlation_id", eventInfo.CorrelationId.Value);
            releaseLockCommand.CommandText =
                $"""
                DELETE FROM {eventLockMetaData.TableName}
                WHERE {eventLogMetaData.CorrelationIdColumnName} = @correlation_id;
                """;
        }
        
        var markAsPublishedCommand = batch.CreateBatchCommand();
        markAsPublishedCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);
        markAsPublishedCommand.CommandText =
            $"""
            UPDATE {eventLogMetaData.TableName}
            SET {eventLogMetaData.PublishedAtColumnName}
            WHERE {eventLogMetaData.IdColumnName} = @id
            """;

        _ = await batch.ExecuteNonQueryAsync(stoppingToken);
    }
    
    private async Task<EventLog?> GetValueAsync(NpgsqlTransaction transaction, CancellationToken stoppingToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.GetEventLogMetaData();
        
        await using var cmd = transaction.Connection!.CreateCommand();
        cmd.Transaction = transaction;
                    
        cmd.CommandText =
            $"""
             SELECT *
             FROM {eventLogMetaData.TableName}
             WHERE {eventLogMetaData.IdColumnName} = @id
             FOR UPDATE SKIP LOCKED
             """;
        
        await using var reader = await cmd.ExecuteReaderAsync(stoppingToken);
        
        if (!reader.HasRows || !await reader.ReadAsync(stoppingToken))
        {
            return null;
        }

        return await ReadEventLogAsync(reader, eventLogMetaData, stoppingToken);
    }

    private static async Task<EventLog> ReadEventLogAsync(NpgsqlDataReader reader,
        EventLogMetaData eventLogMetaData,
        CancellationToken stoppingToken) =>
        new()
        {
            Id = reader.GetGuid(eventLogMetaData.IdColumnName),
            Name = reader.GetString(eventLogMetaData.NameColumnName),
            Payload = reader.GetString(eventLogMetaData.PayloadColumnName),
            Type = reader.GetString(eventLogMetaData.TypeColumnName),
            ActivityId = reader.GetString(eventLogMetaData.ActivityIdColumnName),
            CorrelationId = await reader.IsDBNullAsync(eventLogMetaData.CorrelationIdColumnName, stoppingToken)
                ? null
                : reader.GetGuid(eventLogMetaData.CorrelationIdColumnName),
            CreatedAt = reader.GetDateTime(eventLogMetaData.CreatedAtColumnName),
            OperationName = reader.GetString(eventLogMetaData.OperationNameColumnName),
            UserId = await reader.IsDBNullAsync(eventLogMetaData.UserIdColumnName, stoppingToken)
                ? null
                : reader.GetGuid(eventLogMetaData.UserIdColumnName)
        };

    private async Task<EventLog?> TryAcquireCorrelationLockAsync(NpgsqlTransaction transaction, 
        EventInfo eventInfo,
        CancellationToken stoppingToken)
    {
        var eventLogMetaData = _eventMetaDataProvider.GetEventLogMetaData();
        var eventLockMetaData = _eventMetaDataProvider.GetEventLockMetaData();
        
        await using var batch = transaction.Connection!.CreateBatch();
        batch.Transaction = transaction;
        var insertCommand = batch.CreateBatchCommand();
        insertCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);
        insertCommand.Parameters.AddWithValue("@correlation_id", eventInfo.CorrelationId!.Value);
        
        insertCommand.CommandText = 
            $"""
             INSERT INTO {eventLockMetaData.TableName} ({eventLockMetaData.CorrelationIdColumnName}, {eventLockMetaData.AcquiredAtColumnName})
             SELECT {eventLogMetaData.IdColumnName}, CURRENT_TIMESTAMP
             FROM {eventLogMetaData.TableName}
             WHERE {eventLogMetaData.IdColumnName} = @id
             AND {eventLogMetaData.CorrelationIdColumnName} NOT IN 
                 (SELECT {eventLockMetaData.CorrelationIdColumnName}
                  FROM {eventLockMetaData.TableName}
                  WHERE {eventLockMetaData.CorrelationIdColumnName} = @correlation_id)
             FOR UPDATE SKIP LOCKED
             ON CONFLICT DO NOTHING
             RETURNING 1;
             """;

        var readCommand = batch.CreateBatchCommand();
        readCommand.Parameters.AddWithValue("@id", eventInfo.EventLogId);

        readCommand.CommandText =
            $"""
             SELECT *
             FROM {eventLogMetaData.TableName}
             WHERE {eventLogMetaData.IdColumnName} = @id
             FOR UPDATE SKIP LOCKED
             """;

        await using var reader = await batch.ExecuteReaderAsync(stoppingToken);

        if (!reader.HasRows || !await reader.ReadAsync(stoppingToken))
        {
            return null;
        }

        _ = await reader.NextResultAsync(stoppingToken);
        
        if (!reader.HasRows || !await reader.ReadAsync(stoppingToken))
        {
            return null;
        }

        return await ReadEventLogAsync(reader, eventLogMetaData, stoppingToken);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 5
            })
            .Build();
        
        try
        {
            await foreach (var eventLog in _eventReader.GetUnpublishedEventsAsync(cancellationToken))
            {
                await pipeline.ExecuteAsync(PublishAsync, eventLog, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while fetching unpublished events");
        }
    }

    private async ValueTask PublishAsync(EventLog eventLog, CancellationToken cancellationToken)
    {
        var @event = _eventSerializer.Deserialize(eventLog.Type, eventLog.Payload);

        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            using var activity =
                EventPublisherActivitySource.CreateActivity(eventLog.Name, ActivityKind.Internal);

            activity?.SetParentId(eventLog.ActivityId);
            activity?.Start();
            
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Publish(@event, cancellationToken);
        }
        
        _logger.EventPublished(eventLog.Id);

        if (_eventMapper.TryMap(@event, out var integrationEvent))
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                using var activity =
                    EventPublisherActivitySource.CreateActivity(eventLog.Name, ActivityKind.Internal);

                activity?.SetParentId(eventLog.ActivityId);
                activity?.Start();
            
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Publish(integrationEvent, cancellationToken);
            }
            
            _logger.IntegrationEventPublished(eventLog.Id);
        }
    }
}
