namespace ModularMonolith.Modules.FirstModule.Contracts.Responses;

public class CategoryDetailsResponse
{
    public required Guid Id { get; init; }
    
    public required Guid? ParentId { get; init; }
    
    public required string Name { get; init; }
}
