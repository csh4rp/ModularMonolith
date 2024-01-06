using System.Diagnostics;
using MediatR;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventPublisher : IEventPublisher
{
    private static readonly ActivitySource EventPublisherActivitySource = new(nameof(EventPublisher));

    private readonly EventSerializer _eventSerializer;
    private readonly IServiceProvider _serviceScopeFactory;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(EventSerializer eventSerializer,
        IServiceProvider serviceScopeFactory,
        ILogger<EventPublisher> logger)
    {
        _eventSerializer = eventSerializer;
        _serviceScopeFactory = serviceScopeFactory;
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

        var mappingType = typeof(IEventMapping<>).MakeGenericType(@event.GetType());
        var eventMapping = _serviceScopeFactory.GetService(mappingType);

        if (eventMapping is null)
        {
            return;
        }

        var method = mappingType.GetMethod(nameof(IEventMapping<IEvent>.Map))!;
        var integrationEvent = (IIntegrationEvent)method.Invoke(eventMapping, [@event])!;

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
