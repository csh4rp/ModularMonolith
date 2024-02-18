namespace ModularMonolith.Shared.Application.Exceptions;

public class EntityNotFoundException : ApplicationLogicException
{
    public EntityNotFoundException(Type entityType, object entityId)
        : base($"Entity of type: '{entityType.Name}' with id: '{entityId}' was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public Type EntityType { get; }

    public object EntityId { get; }

    public override string Code => "ENTITY_NOT_FOUND";
}
