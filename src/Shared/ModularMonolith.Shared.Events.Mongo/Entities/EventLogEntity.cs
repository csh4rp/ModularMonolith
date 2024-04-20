using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ModularMonolith.Shared.Events.Mongo.Entities;

public class EventLogEntity
{
    public required Guid Id { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc, Representation = BsonType.String)]
    public required DateTime OccurredAt { get; init; }

    public required Guid? CorrelationId { get; init; }

    public string? Subject { get; init; }

    public required string EventType { get; init; }

    public required BsonDocument EventPayload { get; init; }
}
