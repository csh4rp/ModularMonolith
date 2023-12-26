using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Commands;

public class DeleteCategoryCommand(Guid id) : ICommand
{
    public Guid Id { get; } = id;
}
