using ModularMonolith.CategoryManagement.Contracts.Categories.Shared;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Querying;

public sealed record CategoriesResponse(IReadOnlyList<CategoryItemModel> Items, int TotalLength)
    : IPaginatedResponse<CategoryItemModel>;
