using ModularMonolith.Shared.DataAccess.AudiLog;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLog.Models;

public class AuditLogEntity
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string EntityTypeName { get; init; }

    public required EntityField[] EntityKey { get; init; }

    public required EntityFieldChange[] EntityChanges { get; init; }

    public required AuditOperationType OperationType { get; init; }

    public required AuditLogEntityMetaData MetaData { get; init; }
}
