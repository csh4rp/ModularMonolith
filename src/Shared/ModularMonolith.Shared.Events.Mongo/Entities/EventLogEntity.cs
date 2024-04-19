using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ModularMonolith.Shared.Events.Mongo.Entities;

public class EventLogEntity
{
    [BsonId]
    public required Guid Id { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime OccurredAt { get; init; }

    public string? Subject { get; init; }

    public required string EventType { get; init; }

    [BsonExtraElements]
    public required BsonDocument EventPayload { get; init; }
}
