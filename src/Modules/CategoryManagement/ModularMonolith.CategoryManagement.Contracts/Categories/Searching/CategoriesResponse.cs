using ModularMonolith.CategoryManagement.Contracts.Categories.Models;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Searching;

public sealed record CategoriesResponse(IReadOnlyList<CategoryItemModel> Items, int TotalLength)
    : IPaginatedResponse<CategoryItemModel>;
