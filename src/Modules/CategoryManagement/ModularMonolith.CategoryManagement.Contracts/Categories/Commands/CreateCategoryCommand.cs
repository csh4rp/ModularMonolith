using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Commands;

public sealed record CreateCategoryCommand(Guid? ParentId, string Name) : ICommand<CreatedResponse>;
