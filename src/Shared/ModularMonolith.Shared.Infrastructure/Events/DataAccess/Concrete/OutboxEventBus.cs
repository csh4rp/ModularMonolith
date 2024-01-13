using System.Text.Json;
using MassTransit;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class OutboxEventBus : IEventBus
{
    private static readonly EventPublishOptions DefaultOptions = new();
    
    private readonly IEventLogDatabase _database;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly TimeProvider _timeProvider;
    private readonly IPublishEndpoint _bus;

    public OutboxEventBus(IEventLogDatabase database,
        IIdentityContextAccessor identityContextAccessor,
        TimeProvider timeProvider,
        IPublishEndpoint bus)
    {
        _database = database;
        _identityContextAccessor = identityContextAccessor;
        _timeProvider = timeProvider;
        _bus = bus;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        => PublishAsync(@event, DefaultOptions, cancellationToken);

    public async Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions options, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var identityContext = _identityContextAccessor.Context;
        var messageId = Guid.NewGuid();
        
        await _bus.Publish(@event, a =>
        {
            a.MessageId = messageId;
            a.CorrelationId = options.CorrelationId;
        }, cancellationToken);

        var log = new EventLog
        {
            Id = messageId,
            CreatedAt = _timeProvider.GetUtcNow(),
            EventPayload = JsonSerializer.Serialize(@event),
            EventType = typeof(TEvent).FullName!,
            UserId = identityContext?.UserId
        };

        _database.EventLogs.Add(log);
        await _database.SaveChangesAsync(cancellationToken);
    }

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        => PublishAsync(events, DefaultOptions, cancellationToken);

    public async Task PublishAsync(IEnumerable<IEvent> events, EventPublishOptions options,
        CancellationToken cancellationToken)
    {
        var identityContext = _identityContextAccessor.Context;
        var eventLogs = new List<EventLog>();
        
        foreach (var @event in events)
        {
            var messageId = Guid.NewGuid();
            
            await _bus.Publish(@event, a =>
            {
                a.MessageId = messageId;
                a.CorrelationId = options.CorrelationId;
            }, cancellationToken);

            var eventLog = new EventLog
            {
                Id = messageId,
                CreatedAt = _timeProvider.GetUtcNow(),
                EventPayload = JsonSerializer.Serialize(@event),
                EventType = @event.GetType().FullName!,
                UserId = identityContext?.UserId
            };

            eventLogs.Add(eventLog);
        }

        _database.EventLogs.AddRange(eventLogs);
        await _database.SaveChangesAsync(cancellationToken);
    }
}
