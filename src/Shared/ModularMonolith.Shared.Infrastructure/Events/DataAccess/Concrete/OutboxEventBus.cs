using System.Reflection;
using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class OutboxEventBus : IEventBus
{
    private static readonly EventPublishOptions DefaultOptions = new();
    
    private readonly DbContext _database;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly TimeProvider _timeProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IServiceProvider _serviceProvider;

    public OutboxEventBus(DbContext database,
        IIdentityContextAccessor identityContextAccessor,
        TimeProvider timeProvider,
        IPublishEndpoint publishEndpoint,
        IServiceProvider serviceProvider)
    {
        _database = database;
        _identityContextAccessor = identityContextAccessor;
        _timeProvider = timeProvider;
        _publishEndpoint = publishEndpoint;
        _serviceProvider = serviceProvider;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        => PublishAsync(@event, DefaultOptions, cancellationToken);

    public async Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions options, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var identityContext = _identityContextAccessor.Context;
        var type = typeof(TEvent);
        var messageId = Guid.NewGuid();
        
        var attribute = type.GetCustomAttribute<EventAttribute>();

        if (attribute?.IsPersisted is true)
        {
            var eventLog = new EventLog
            {
                Id = messageId,
                CreatedAt = _timeProvider.GetUtcNow(),
                EventPayload = JsonSerializer.Serialize(@event),
                EventType = type.FullName!,
                UserId = identityContext?.UserId
            };

            _database.Set<EventLog>().Add(eventLog);
        }
        
        await _publishEndpoint.Publish(@event, a =>
        {
            a.MessageId = messageId;
            a.CorrelationId = options.CorrelationId;
        }, cancellationToken);

        var mappingType = typeof(IEventMapping<>).MakeGenericType(type);
        var mapping = _serviceProvider.GetService(mappingType);

        if (mapping is not null)
        {
            var method = mappingType.GetMethod(nameof(IEventMapping<IEvent>.Map))!;
            var integrationEvent = (IIntegrationEvent) method.Invoke(mapping, [@event])!;

            await _publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), cnx =>
            {
                cnx.CorrelationId = options.CorrelationId;
                cnx.InitiatorId = messageId;
            }, cancellationToken);
        }
        
        await _database.SaveChangesAsync(cancellationToken);
    }

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        => PublishAsync(events, DefaultOptions, cancellationToken);

    public async Task PublishAsync(IEnumerable<IEvent> events, 
        EventPublishOptions options,
        CancellationToken cancellationToken)
    {
        var identityContext = _identityContextAccessor.Context;
        var eventLogs = new List<EventLog>();

        foreach (var @event in events)
        {
            var messageId = Guid.NewGuid();
            var type = @event.GetType();
            var attribute = type.GetCustomAttribute<EventAttribute>();
            
            if (attribute?.IsPersisted is true)
            {
                var eventLog = new EventLog
                {
                    Id = messageId,
                    CreatedAt = _timeProvider.GetUtcNow(),
                    EventPayload = JsonSerializer.Serialize(@event),
                    EventType = type.FullName!,
                    UserId = identityContext?.UserId
                };

                eventLogs.Add(eventLog);
            }
            
            await _publishEndpoint.Publish(@event, a =>
            {
                a.MessageId = messageId;
                a.CorrelationId = options.CorrelationId;
            }, cancellationToken);
            
            var mappingType = typeof(IEventMapping<>).MakeGenericType(type);
            var mapping = _serviceProvider.GetService(mappingType);

            if (mapping is not null)
            {
                var method = mappingType.GetMethod(nameof(IEventMapping<IEvent>.Map))!;
                var integrationEvent = (IIntegrationEvent) method.Invoke(mapping, [@event])!;

                await _publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), cnx =>
                {
                    cnx.CorrelationId = options.CorrelationId;
                    cnx.InitiatorId = messageId;
                }, cancellationToken);
            }
        }

        _database.Set<EventLog>().AddRange(eventLogs);
        await _database.SaveChangesAsync(cancellationToken);
    }
}
