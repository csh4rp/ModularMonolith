namespace ModularMonolith.CategoryManagement.Contracts.Categories.Querying;

public sealed record CategoryDetailsResponse
{
    public required Guid Id { get; init; }

    public required Guid? ParentId { get; init; }

    public required string Name { get; init; }
}
