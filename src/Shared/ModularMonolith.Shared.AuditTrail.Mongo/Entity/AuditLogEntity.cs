using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ModularMonolith.Shared.AuditTrail.Mongo.Entity;

public class AuditLogEntity
{
    public required Guid Id { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc, Representation = BsonType.String)]
    public required DateTime CreatedAt { get; init; }

    public required string EntityType { get; init; }

    public required EntityState EntityState { get; init; }

    public required List<PropertyChange> EntityPropertyChanges { get; init; }

    public required List<EntityKey> EntityKeys { get; init; }

    public string? Subject { get; init; }

    public required string OperationName { get; init; }

    public required string TraceId { get; init; }

    public required string SpanId { get; init; }

    public required string? ParentSpanId { get; init; }

    public required string? IpAddress { get; init; }

    public required string? UserAgent { get; init; }
}
