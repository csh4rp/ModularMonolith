using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.Shared.Infrastructure.Events;

internal sealed class EventBus(DbContext dbContext,
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

        var payload = JsonSerializer.Serialize(@event);

        var eventLog = new EventLog
        {
            CreatedAt = timeProvider.GetUtcNow(),
            Name = attribute?.Name ?? eventType.Name,
            Type = eventType.FullName!,
            Topic = attribute?.Topic,
            Stream = options.Stream,
            Payload = payload,
            UserId = identityContextAccessor.Context?.UserId,
            OperationName = Activity.Current.OperationName,
            TraceId = Activity.Current.TraceId.ToString()
        };

        dbContext.Set<EventLog>().Add(eventLog);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        => PublishAsync(events, DefaultOptions, cancellationToken);
    
    public Task PublishAsync(IEnumerable<IEvent> events, EventPublishOptions options, CancellationToken cancellationToken)
    {
        Debug.Assert(Activity.Current is not null);

        var now = timeProvider.GetUtcNow();

        var logs = events.Select(e =>
        {
            var eventType = e.GetType();
            var attribute = eventType.GetCustomAttribute<EventAttribute>();

            var payload = JsonSerializer.Serialize(e, eventType);

            return new EventLog
            {
                CreatedAt = now,
                Name = attribute?.Name ?? eventType.Name,
                Type = eventType.FullName!,
                Topic = attribute?.Topic,
                Stream = options.Stream,
                Payload = payload,
                UserId = identityContextAccessor.Context?.UserId,
                OperationName = Activity.Current.OperationName,
                TraceId = Activity.Current.TraceId.ToString()
            };
        });

        dbContext.Set<EventLog>().AddRange(logs);
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
