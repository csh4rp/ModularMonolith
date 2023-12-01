using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.FirstModule.Contracts.Commands;

public class DeleteCategoryCommand(Guid id) : ICommand
{
    public Guid Id { get; } = id;
}
