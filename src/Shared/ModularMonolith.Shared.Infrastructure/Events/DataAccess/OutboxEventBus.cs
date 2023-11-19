using System.Diagnostics;
using System.Reflection;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.Utils;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal sealed class OutboxEventBus(IEventLogContext dbContext,
        EventSerializer eventSerializer,
        IIdentityContextAccessor identityContextAccessor,
        TimeProvider timeProvider) : IEventBus
{
    private static readonly EventPublishOptions DefaultOptions = new();

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        => PublishAsync(@event, DefaultOptions, cancellationToken);
    
    public Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions options, CancellationToken cancellationToken) where TEvent : IEvent
    {
        Debug.Assert(Activity.Current is not null);
        
        var eventType = typeof(TEvent);
        var attribute = eventType.GetCustomAttribute<EventAttribute>();
        
        var eventLog = new EventLog
        {
            CreatedAt = timeProvider.GetUtcNow(),
            Name = attribute?.Name ?? eventType.Name,
            Type = eventType.FullName!,
            CorrelationId = options.CorrelationId,
            Payload = eventSerializer.Serialize(@event),
            UserId = identityContextAccessor.Context?.UserId,
            OperationName = Activity.Current.OperationName,
            TraceId = Activity.Current.Id!
        };

        dbContext.EventLogs.Add(eventLog);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        => PublishAsync(events, DefaultOptions, cancellationToken);
    
    public Task PublishAsync(IEnumerable<IEvent> events, EventPublishOptions options, CancellationToken cancellationToken)
    {
        Debug.Assert(Activity.Current is not null);

        var now = timeProvider.GetUtcNow();

        var eventLogs = events.Select(e =>
        {
            var eventType = e.GetType();
            var attribute = eventType.GetCustomAttribute<EventAttribute>();
            
            return new EventLog
            {
                CreatedAt = now,
                Name = attribute?.Name ?? eventType.Name,
                Type = eventType.FullName!,
                CorrelationId = options.CorrelationId,
                Payload = eventSerializer.Serialize(e, eventType),
                UserId = identityContextAccessor.Context?.UserId,
                OperationName = Activity.Current.OperationName,
                TraceId = Activity.Current.Id!
            };
        });

        dbContext.EventLogs.AddRange(eventLogs);
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
