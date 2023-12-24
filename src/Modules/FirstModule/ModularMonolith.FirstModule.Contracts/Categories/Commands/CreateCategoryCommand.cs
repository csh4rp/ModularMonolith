using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.FirstModule.Contracts.Categories.Commands;

public class CreateCategoryCommand : ICommand<CreatedResponse>
{
    public required Guid? ParentId { get; init; }

    public required string Name { get; init; }
}
