namespace ModularMonolith.CategoryManagement.Contracts.Categories.Shared;

public sealed record CategoryItemModel
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}
