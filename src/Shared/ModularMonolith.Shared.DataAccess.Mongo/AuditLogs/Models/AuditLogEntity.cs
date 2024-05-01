using ModularMonolith.Shared.AuditTrail;
using MongoDB.Bson;

namespace ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Models;

public class AuditLogEntity
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string EntityType { get; init; }

    public required EntityState EntityState { get; init; }

    public required BsonDocument EntityPropertyChanges { get; init; }

    public required BsonDocument EntityKeys { get; init; }

    public required AuditMetaData MetaData { get; init; }
}
