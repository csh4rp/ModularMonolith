using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ModularMonolith.Shared.AuditTrail.Mongo.Model;

public class AuditLogEntity
{
    public required Guid Id { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime CreatedAt { get; init; }

    public required string EntityType { get; init; }

    public required EntityState EntityState { get; init; }

    public required BsonDocument EntityPropertyChanges { get; init; }

    public required BsonDocument EntityKeys { get; init; }

    public required AuditMetaData MetaData { get; init; }
}
