using System.Diagnostics;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventPublisher
{
    private static readonly ActivitySource EventPublisherActivitySource = new(nameof(EventPublisher));

    private readonly EventSerializer _eventSerializer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly EventMapper _eventMapper;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(EventSerializer eventSerializer,
        IServiceScopeFactory serviceScopeFactory,
        EventMapper eventMapper,
        ILogger<EventPublisher> logger)
    {
        _eventSerializer = eventSerializer;
        _serviceScopeFactory = serviceScopeFactory;
        _eventMapper = eventMapper;
        _logger = logger;
    }

    public async Task PublishAsync(EventLog eventLog, CancellationToken cancellationToken)
    {
        var @event = _eventSerializer.Deserialize(eventLog.Type, eventLog.Payload);

        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            if (eventLog.UserId.HasValue)
            {
                var identitySetter = scope.ServiceProvider.GetRequiredService<IIdentityContextSetter>();
                identitySetter.Set(new IdentityContext(eventLog.UserId.Value));
            }

            using var activity = EventPublisherActivitySource.CreateActivity(eventLog.Name, ActivityKind.Internal);
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
                if (eventLog.UserId.HasValue)
                {
                    var identitySetter = scope.ServiceProvider.GetRequiredService<IIdentityContextSetter>();
                    identitySetter.Set(new IdentityContext(eventLog.UserId.Value));
                }

                using var activity = EventPublisherActivitySource.CreateActivity(eventLog.Name, ActivityKind.Internal);
                activity?.SetParentId(eventLog.ActivityId);
                activity?.Start();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Publish(integrationEvent, cancellationToken);
            }

            _logger.IntegrationEventPublished(eventLog.Id);
        }
    }
}
