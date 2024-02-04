using ModularMonolith.CategoryManagement.Contracts.Categories.Models;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Attributes;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Searching;

public sealed class FindCategoriesQuery : IQuery<CategoriesResponse>, IPaginatedQuery
{
    public required string? Search { get; set; }

    [DefaultOrderBy(nameof(CategoryItemModel.Name))]
    public required string? OrderBy { get; set; }

    public required int? Skip { get; set; }

    [DefaultTake(100)]
    public required int? Take { get; set; }
}
