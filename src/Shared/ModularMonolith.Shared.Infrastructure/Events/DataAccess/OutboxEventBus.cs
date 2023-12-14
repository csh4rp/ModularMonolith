using System.Reflection;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.Utils;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal sealed class OutboxEventBus : IEventBus
{
    private static readonly EventPublishOptions DefaultOptions = new();
    private readonly IEventLogContext _dbContext;
    private readonly EventSerializer _eventSerializer;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly TimeProvider _timeProvider;

    public OutboxEventBus(IEventLogContext dbContext,
        EventSerializer eventSerializer,
        IIdentityContextAccessor identityContextAccessor,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _eventSerializer = eventSerializer;
        _identityContextAccessor = identityContextAccessor;
        _timeProvider = timeProvider;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        => PublishAsync(@event, DefaultOptions, cancellationToken);
    
    public Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions options, CancellationToken cancellationToken) where TEvent : IEvent
    {
        var currentActivity = System.Diagnostics.Activity.Current;
        System.Diagnostics.Debug.Assert(currentActivity is not null);
        
        var eventType = typeof(TEvent);
        var attribute = eventType.GetCustomAttribute<EventAttribute>();
        
        var eventLog = new EventLog
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            Name = attribute?.Name ?? eventType.Name,
            Type = eventType.FullName!,
            CorrelationId = options.CorrelationId,
            Payload = _eventSerializer.Serialize(@event),
            UserId = _identityContextAccessor.Context?.UserId,
            OperationName = currentActivity.OperationName,
            ActivityId = currentActivity.Id!
        };

        _dbContext.EventLogs.Add(eventLog);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        => PublishAsync(events, DefaultOptions, cancellationToken);
    
    public Task PublishAsync(IEnumerable<IEvent> events, EventPublishOptions options, CancellationToken cancellationToken)
    {
        var currentActivity = System.Diagnostics.Activity.Current;
        System.Diagnostics.Debug.Assert(currentActivity is not null);
        
        var eventLogs = events.Select(e =>
        {
            var eventType = e.GetType();
            var attribute = eventType.GetCustomAttribute<EventAttribute>();
            
            return new EventLog
            {
                CreatedAt = _timeProvider.GetUtcNow(),
                Name = attribute?.Name ?? eventType.Name,
                Type = eventType.FullName!,
                CorrelationId = options.CorrelationId,
                Payload = _eventSerializer.Serialize(e, eventType),
                UserId = _identityContextAccessor.Context?.UserId,
                OperationName = currentActivity.OperationName,
                ActivityId = currentActivity.Id!
            };
        });

        _dbContext.EventLogs.AddRange(eventLogs);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
