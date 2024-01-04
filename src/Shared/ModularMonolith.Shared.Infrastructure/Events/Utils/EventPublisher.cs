using System.Diagnostics;
using MediatR;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventPublisher : IEventPublisher
{
    private static readonly ActivitySource EventPublisherActivitySource = new(nameof(EventPublisher));

    private readonly EventSerializer _eventSerializer;
    private readonly IServiceProvider _serviceScopeFactory;
    private readonly EventMapper _eventMapper;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(EventSerializer eventSerializer,
        IServiceProvider serviceScopeFactory,
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
        var @event = _eventSerializer.Deserialize(eventLog.EventType, eventLog.EventPayload);

        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            if (eventLog.UserId.HasValue)
            {
                var identitySetter = scope.ServiceProvider.GetRequiredService<IIdentityContextSetter>();
                identitySetter.Set(new IdentityContext(eventLog.UserId.Value, eventLog.UserName!));
            }

            using var activity = EventPublisherActivitySource.CreateActivity(eventLog.EventName, ActivityKind.Internal);
            activity!.SetParentId(eventLog.TraceId);
            activity.Start();

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
                    identitySetter.Set(new IdentityContext(eventLog.UserId.Value, eventLog.UserName!));
                }

                using var activity =
                    EventPublisherActivitySource.CreateActivity(eventLog.EventName, ActivityKind.Internal);
                activity?.SetParentId(eventLog.TraceId);
                activity?.Start();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Publish(integrationEvent, cancellationToken);
            }

            _logger.IntegrationEventPublished(eventLog.Id);
        }
    }
}
