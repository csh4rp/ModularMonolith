using System.Text.Json;
using ModularMonolith.Shared.DataAccess.AudiLog;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Entities;

public class AuditLogEntity
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string EntityTypeName { get; init; }

    public required JsonDocument EntityKey { get; init; }

    public required JsonDocument EntityChanges { get; init; }

    public required AuditOperationType OperationType { get; init; }

    public required AuditLogEntityMetaData MetaData { get; init; }
}
