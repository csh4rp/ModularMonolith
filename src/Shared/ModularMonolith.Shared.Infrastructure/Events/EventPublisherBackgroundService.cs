using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModularMonolith.Shared.Infrastructure.Events;

public class EventPublisherBackgroundService(EventReader eventReader, 
        EventSerializer eventSerializer,
        EventMapper eventMapper,
        IBus bus,
        ILogger<EventPublisherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var eventLog in eventReader.GetUnpublishedEventsAsync(stoppingToken))
            {
                try
                {
                    await PublishAsync(eventLog, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "");
        }
    }

    private async Task PublishAsync(EventLog eventLog, CancellationToken cancellationToken)
    {
        var @event = eventSerializer.Deserialize(eventLog.Type, eventLog.Payload);

        await bus.Publish(@event, cnx =>
        {
            cnx.MessageId = eventLog.Id;
            cnx.CorrelationId = eventLog.CorrelationId;
        }, cancellationToken);
        
        logger.EventPublished(eventLog.Id);

        if (eventMapper.TryMap(@event, out var integrationEvent))
        {
            await bus.Publish(integrationEvent, cnx =>
            {
                cnx.MessageId = eventLog.Id;
                cnx.CorrelationId = eventLog.CorrelationId;
            }, cancellationToken);
            
            logger.IntegrationEventPublished(eventLog.Id);
        }
    }
}
