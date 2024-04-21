namespace ModularMonolith.Shared.AuditTrail;

public class AuditLog
{
    public Guid Id { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required string EntityType { get; init; }

    public required EntityState EntityState { get; init; }

    public required List<PropertyChange> EntityPropertyChanges { get; init; }

    public required List<EntityKey> EntityKeys { get; init; }

    public required AuditMetaData MetaData { get; init; }
}
