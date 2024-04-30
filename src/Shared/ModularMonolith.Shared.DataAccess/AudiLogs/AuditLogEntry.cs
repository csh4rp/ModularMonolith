namespace ModularMonolith.Shared.DataAccess.AudiLogs;

public record AuditLogEntry
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required Type EntityType { get; init; }

    public required EntityKey EntityKey { get; init; }

    public required EntityChanges EntityChanges { get; init; }

    public required AuditOperationType OperationType { get; init; }

    public required AuditMetaData MetaData { get; init; }
}
