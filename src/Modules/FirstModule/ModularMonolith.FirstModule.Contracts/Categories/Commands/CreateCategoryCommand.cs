using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.FirstModule.Contracts.Categories.Commands;

public sealed record CreateCategoryCommand(Guid? ParentId, string Name) : ICommand<CreatedResponse>;
