using System.Collections.Concurrent;
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
        var eventLogMetaData = _eventMetaDataProvider.GetEventLogMetaData();
        var eventLockMetaData = _eventMetaDataProvider.GetEventLockMetaData();
        
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            _tasks[i] = Task.Factory.StartNew(async () =>
            {
                await foreach (var eventInfo in _eventChannel.ReadAllAsync(stoppingToken))
                {
                    await using var connection = _dbConnectionFactory.Create();
                    await connection.OpenAsync(stoppingToken);

                    await using var transaction = await connection.BeginTransactionAsync(stoppingToken);

                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@id", eventInfo.EventLogId);
                    cmd.CommandText = 
                        $"""
                         INSERT INTO {eventLockMetaData.TableName} ({eventLockMetaData.CorrelationIdColumnName}, {eventLockMetaData.AcquiredAtColumnName})
                         SELECT {eventLogMetaData.IdColumnName}, CURRENT_TIMESTAMP
                         FROM {eventLogMetaData.TableName}
                         WHERE {eventLogMetaData.IdColumnName} = @id
                         AND {eventLogMetaData.CorrelationIdColumnName} IS NOT NULL
                         FOR UPDATE SKIP LOCKED
                         ON CONFLICT DO NOTHING
                         RETURNING 1;
                         """;
                    
                    await using (var reader = await cmd.ExecuteReaderAsync(stoppingToken))
                    {
                        if (!reader.HasRows)
                        {
                            // Lock was already acquired
                            continue;
                        }
                    }
                    
                    cmd.CommandText =
                        $"""
                        SELECT *
                        FROM {eventLogMetaData.TableName}
                        WHERE {eventLogMetaData.IdColumnName} = @id
                        FOR UPDATE SKIP LOCKED
                        """;


                    EventLog? eventLog;
                    
                    await using (var reader = await cmd.ExecuteReaderAsync(stoppingToken))
                    {
                        if (!reader.HasRows)
                        {
                            // Lock was already acquired
                            continue;
                        }

                        await reader.ReadAsync(stoppingToken);

                        eventLog= new EventLog
                        {
                            Id = default,
                            Name = default,
                            Payload = default,
                            Type = default,
                            ActivityId = default,
                            CorrelationId = default,
                            CreatedAt = default,
                            OperationName = default,
                            UserId = default
                        };
                    }

                    try
                    {
                        await using var scope = _serviceScopeFactory.CreateAsyncScope();

                        var @event = _eventSerializer.Deserialize(eventLog.Type, eventLog.Payload);

                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        await mediator.Publish(@event, stoppingToken);
                        
                        await transaction.CommitAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
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

    private async ValueTask RunAsync(CancellationToken cancellationToken)
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
