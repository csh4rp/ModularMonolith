using ModularMonolith.Shared.Domain.Enums;
using ModularMonolith.Shared.Domain.ValueObjects;

namespace ModularMonolith.Shared.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required string EntityType { get; init; }

    public required EntityState EntityState { get; init; }

    public required List<PropertyChange> EntityPropertyChanges { get; init; }

    public required List<EntityKey> EntityKeys { get; init; }

    public Guid? UserId { get; init; }

    public required string OperationName { get; init; }

    public required string ActivityId { get; init; }
}
