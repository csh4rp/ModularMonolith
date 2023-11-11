using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Core;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.Shared.Infrastructure.Events;

internal sealed class EventBus : IEventBus
{
    private static readonly EventPublishOptions DefaultOptions = new();
    
    private readonly DbContext _dbContext;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EventBus(DbContext dbContext, IIdentityContextAccessor identityContextAccessor, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _identityContextAccessor = identityContextAccessor;
        _dateTimeProvider = dateTimeProvider;
    }

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
            CreatedAt = _dateTimeProvider.GetUtcNow(),
            Name = attribute?.Name ?? eventType.Name,
            Type = eventType.FullName!,
            Topic = attribute?.Topic,
            Stream = options.Stream,
            Payload = payload,
            UserId = _identityContextAccessor.Context?.UserId,
            OperationName = Activity.Current.OperationName,
            TraceId = Activity.Current.TraceId.ToString()
        };

        _dbContext.Set<EventLog>().Add(eventLog);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        => PublishAsync(events, DefaultOptions, cancellationToken);
    
    public Task PublishAsync(IEnumerable<IEvent> events, EventPublishOptions options, CancellationToken cancellationToken)
    {
        Debug.Assert(Activity.Current is not null);

        var now = _dateTimeProvider.GetUtcNow();

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
                UserId = _identityContextAccessor.Context?.UserId,
                OperationName = Activity.Current.OperationName,
                TraceId = Activity.Current.TraceId.ToString()
            };
        });

        _dbContext.Set<EventLog>().AddRange(logs);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
