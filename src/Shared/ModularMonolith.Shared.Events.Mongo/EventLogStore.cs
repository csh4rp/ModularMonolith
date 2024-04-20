using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Events.Mongo.Entities;
using ModularMonolith.Shared.Events.Mongo.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.Events.Mongo;

internal sealed class EventLogStore : IEventLogStore
{
    private readonly IMongoCollection<EventLogEntity> _mongoCollection;

    public EventLogStore(IMongoDatabase mongoDatabase, IOptions<EventOptions> optionsMonitor)
    {
        _mongoCollection = mongoDatabase.GetCollection<EventLogEntity>(optionsMonitor.Value.CollectionName);
    }

    public async Task<TEvent?> FindFirstOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var sortDefinition = Builders<EventLogEntity>.Sort.Ascending(f => f.OccurredAt);

        using var cursor = await _mongoCollection.FindAsync(f => f.Subject == subject,
            new FindOptions<EventLogEntity> { Sort = sortDefinition, Limit = 1 }, cancellationToken);

        var log = cursor.Current.FirstOrDefault();
        return log is null ? default : BsonSerializer.Deserialize<TEvent>(log.EventPayload);
    }

    public async Task<EventLog?> FindFirstOccurenceAsync(string subject, Type eventType,
        CancellationToken cancellationToken)
    {
        var sortDefinition = Builders<EventLogEntity>.Sort.Ascending(f => f.OccurredAt);

        using var cursor = await _mongoCollection.FindAsync(f => f.Subject == subject,
            new FindOptions<EventLogEntity> { Sort = sortDefinition, Limit = 1 }, cancellationToken);

        var log = cursor.Current.FirstOrDefault();

        return log is null
            ? default
            : new EventLog
            {
                Id = log.Id,
                EventType = log.EventType,
                EventPayload = log.EventPayload.ToString(),
                OccurredAt = new DateTimeOffset(log.OccurredAt, TimeSpan.Zero),
                Subject = log.Subject
            };
    }

    public async Task<TEvent?> FindLastOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var sortDefinition = Builders<EventLogEntity>.Sort.Descending(f => f.OccurredAt);

        using var cursor = await _mongoCollection.FindAsync(f => f.Subject == subject,
            new FindOptions<EventLogEntity> { Sort = sortDefinition, Limit = 1 }, cancellationToken);

        var log = cursor.Current.FirstOrDefault();
        return log is null ? default : BsonSerializer.Deserialize<TEvent>(log.EventPayload);
    }

    public async Task<EventLog?> FindLastOccurenceAsync(string subject, Type eventType, CancellationToken cancellationToken)
    {
        var sortDefinition = Builders<EventLogEntity>.Sort.Descending(f => f.OccurredAt);

        using var cursor = await _mongoCollection.FindAsync(f => f.Subject == subject,
            new FindOptions<EventLogEntity> { Sort = sortDefinition, Limit = 1 }, cancellationToken);

        var log = cursor.Current.FirstOrDefault();

        return log is null
            ? default
            : new EventLog
            {
                Id = log.Id,
                EventType = log.EventType,
                EventPayload = log.EventPayload.ToString(),
                OccurredAt = new DateTimeOffset(log.OccurredAt, TimeSpan.Zero),
                Subject = log.Subject
            };
    }
}
