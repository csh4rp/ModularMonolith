using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Polly;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventPublisherBackgroundService : BackgroundService
{
    private readonly EventReader _eventReader;
    private readonly EventSerializer _eventSerializer;
    private readonly EventMapper _eventMapper;
    private readonly IBus _bus;
    private readonly ILogger<EventPublisherBackgroundService> _logger;

    public EventPublisherBackgroundService(EventReader eventReader, 
        EventSerializer eventSerializer,
        EventMapper eventMapper,
        IBus bus,
        ILogger<EventPublisherBackgroundService> logger)
    {
        _eventReader = eventReader;
        _eventSerializer = eventSerializer;
        _eventMapper = eventMapper;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

        await _bus.Publish(@event, cnx =>
        {
            cnx.MessageId = eventLog.Id;
            cnx.CorrelationId = eventLog.CorrelationId;
        }, cancellationToken);
        
        Extensions.EventLoggingExtensions.EventPublished(_logger, eventLog.Id);

        if (_eventMapper.TryMap(@event, out var integrationEvent))
        {
            await _bus.Publish(integrationEvent, cnx =>
            {
                cnx.MessageId = eventLog.Id;
                cnx.CorrelationId = eventLog.CorrelationId;
                cnx.Headers.Set("TraceId", eventLog.ActivityId);
            }, cancellationToken);
            
            Extensions.EventLoggingExtensions.IntegrationEventPublished(_logger, eventLog.Id);
        }
    }
}
