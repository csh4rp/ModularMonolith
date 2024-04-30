using MongoDB.Bson;

namespace ModularMonolith.Shared.DataAccess.Mongo.Outbox.Models;

public class OutboxMessage
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string MessageTypeName { get; init; }

    public required BsonDocument MessagePayload { get; init; }
}
