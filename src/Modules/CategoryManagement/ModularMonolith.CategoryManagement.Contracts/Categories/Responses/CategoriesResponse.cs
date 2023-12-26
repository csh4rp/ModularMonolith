using ModularMonolith.CategoryManagement.Contracts.Categories.Models;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Responses;

public class CategoriesResponse : IPaginatedResponse<CategoryItemModel>
{
    public required IReadOnlyList<CategoryItemModel> Items { get; init; }

    public required int TotalLength { get; init; }
}
