using ModularMonolith.Shared.Events.Mongo.Entities;
using ModularMonolith.Shared.Identity;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.Events.Mongo;

internal sealed class EventBus : IEventBus
{
    private static readonly EventPublishOptions DefaultOptions = new();
    private static readonly InsertOneOptions DefaultInsertOptions = new();
    private static readonly InsertManyOptions DefaultInsertManyOptions = new();

    private readonly IMongoCollection<EventLogEntity> _eventLogs;
    private readonly IIdentityContextAccessor _identityContextAccessor;

    public EventBus(IMongoDatabase mongoDatabase, IIdentityContextAccessor identityContextAccessor)
    {
        _identityContextAccessor = identityContextAccessor;
        _eventLogs = mongoDatabase.GetCollection<EventLogEntity>("EventLog");
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        => PublishAsync(@event, DefaultOptions, cancellationToken);

    public async Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions options, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventLog = CreateEventLog(@event, options);
        await _eventLogs.InsertOneAsync(eventLog, DefaultInsertOptions, cancellationToken);
    }

    private EventLogEntity CreateEventLog<TEvent>(TEvent @event, EventPublishOptions options) where TEvent : IEvent =>
        new()
        {
            Id = Guid.NewGuid(),
            CorrelationId = options.CorrelationId,
            OccurredAt = @event.OccurredAt.UtcDateTime,
            EventPayload = Serialize(@event),
            EventType = typeof(TEvent).FullName!,
            Subject = _identityContextAccessor.IdentityContext?.Subject,
        };

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken) =>
        PublishAsync(events, DefaultOptions, cancellationToken);

    public Task PublishAsync(IEnumerable<IEvent> events,
        EventPublishOptions options,
        CancellationToken cancellationToken)
    {
        var eventLogs = events.Select(e => CreateEventLog(e, options));
        return _eventLogs.InsertManyAsync(eventLogs, DefaultInsertManyOptions, cancellationToken);
    }

    private static BsonDocument Serialize<TEvent>(TEvent @event) where TEvent : IEvent
    {
        using var ms = new MemoryStream();
        using var writer = new BsonBinaryWriter(ms);
        BsonSerializer.Serialize(writer, @event);
        return writer.ToBsonDocument();
    }
}
