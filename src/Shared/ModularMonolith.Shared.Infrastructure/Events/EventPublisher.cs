using MassTransit;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModularMonolith.Shared.Infrastructure.Events;

public class EventPublisher : BackgroundService
{
    private readonly EventReader _eventReader;
    private readonly IBus _publisher;
    private readonly ILogger<EventPublisher> _logger;
    
    public EventPublisher(EventReader eventReader, ILogger<EventPublisher> logger)
    {
        _eventReader = eventReader;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var eventLog in _eventReader.GetUnpublishedEventsAsync(stoppingToken))
            {
                
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");
        }
    }
}
