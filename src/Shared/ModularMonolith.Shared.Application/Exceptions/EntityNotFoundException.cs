namespace ModularMonolith.Shared.Application.Exceptions;

public class EntityNotFoundException : AppException
{
    public EntityNotFoundException(Type entityType, Guid entityId)
        : base($"Entity of type: '{entityType.Name}' with id: '{entityId}' was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public Type EntityType { get; }

    public Guid EntityId { get; }

    public override string Code => "ENTITY_NOT_FOUND";
}
