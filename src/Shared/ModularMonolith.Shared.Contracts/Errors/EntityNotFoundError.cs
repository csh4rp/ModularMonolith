namespace ModularMonolith.Shared.Contracts.Errors;

public sealed class EntityNotFoundError : Error
{
    public EntityNotFoundError(string entityName, object entityId)
        : base(ErrorCodes.EntityNotFound, $"Entity: '{entityName}' with ID: '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
    
    public string EntityName { get; init; }
    
    public object EntityId { get; init; }
}
