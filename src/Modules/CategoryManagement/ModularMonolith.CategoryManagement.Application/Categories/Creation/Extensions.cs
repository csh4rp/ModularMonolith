using ModularMonolith.CategoryManagement.Contracts.Categories.Creation;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Application.Categories.Creation;

public static class Extensions
{
    public static Category ToCategory(this CreateCategoryCommand command) =>
        Category.Create(command.ParentId.HasValue ? CategoryId.From(command.ParentId.Value) : null, command.Name);
}
