using System.Diagnostics;
using System.Reflection;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class OutboxEventBus : IEventBus
{
    private static readonly EventPublishOptions DefaultOptions = new();
    private readonly IEventLogDbContext _dbContext;
    private readonly EventSerializer _eventSerializer;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly TimeProvider _timeProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OutboxEventBus(IEventLogDbContext dbContext,
        EventSerializer eventSerializer,
        IIdentityContextAccessor identityContextAccessor,
        TimeProvider timeProvider, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _eventSerializer = eventSerializer;
        _identityContextAccessor = identityContextAccessor;
        _timeProvider = timeProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        => PublishAsync(@event, DefaultOptions, cancellationToken);

    public Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions options, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var currentActivity = Activity.Current;
        Debug.Assert(currentActivity is not null);

        var eventType = typeof(TEvent);
        var attribute = eventType.GetCustomAttribute<EventAttribute>();

        var identityContext = _identityContextAccessor.Context;
        var remoteIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent;

        var eventLog = new EventLog
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            EventName = attribute?.Name ?? eventType.Name,
            EventType = eventType.FullName!,
            EventPayload = _eventSerializer.Serialize(@event),
            CorrelationId = options.CorrelationId,
            UserId = identityContext?.UserId,
            UserName = identityContext?.UserName,
            OperationName = currentActivity.OperationName,
            TraceId = currentActivity.TraceId.ToString(),
            SpanId = currentActivity.SpanId.ToString(),
            ParentSpanId = currentActivity.Parent is null ? null : currentActivity.ParentSpanId.ToString(),
            IpAddress = remoteIpAddress?.ToString(),
            UserAgent = userAgent,
        };

        var scope = TransactionalScope.Current.Value;
        if (scope?.DbContext is not null)
        {
            scope.DbContext.Set<EventLog>().Add(eventLog);
            return scope.DbContext.SaveChangesAsync(cancellationToken);
        }

        _dbContext.EventLogs.Add(eventLog);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        => PublishAsync(events, DefaultOptions, cancellationToken);

    public Task PublishAsync(IEnumerable<IEvent> events, EventPublishOptions options,
        CancellationToken cancellationToken)
    {
        var currentActivity = Activity.Current;
        Debug.Assert(currentActivity is not null);

        var identityContext = _identityContextAccessor.Context;
        var remoteIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent;

        var eventLogs = events.Select(e =>
        {
            var eventType = e.GetType();
            var attribute = eventType.GetCustomAttribute<EventAttribute>();

            return new EventLog
            {
                CreatedAt = _timeProvider.GetUtcNow(),
                EventName = attribute?.Name ?? eventType.Name,
                EventType = eventType.FullName!,
                EventPayload = _eventSerializer.Serialize(e),
                CorrelationId = options.CorrelationId,
                UserId = identityContext?.UserId,
                UserName = identityContext?.UserName,
                OperationName = currentActivity.OperationName,
                TraceId = currentActivity.TraceId.ToString(),
                SpanId = currentActivity.SpanId.ToString(),
                ParentSpanId = currentActivity.Parent is null ? null : currentActivity.ParentSpanId.ToString(),
                IpAddress = remoteIpAddress?.ToString(),
                UserAgent = userAgent,
            };
        });

        var scope = TransactionalScope.Current.Value;
        if (scope?.DbContext is not null)
        {
            scope.DbContext.Set<EventLog>().AddRange(eventLogs);
            return scope.DbContext.SaveChangesAsync(cancellationToken);
        }

        _dbContext.EventLogs.AddRange(eventLogs);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
