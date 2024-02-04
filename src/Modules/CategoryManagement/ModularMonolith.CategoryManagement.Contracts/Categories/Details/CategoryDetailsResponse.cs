namespace ModularMonolith.CategoryManagement.Contracts.Categories.Details;

public sealed record CategoryDetailsResponse
{
    public required Guid Id { get; init; }

    public required Guid? ParentId { get; init; }

    public required string Name { get; init; }
}
