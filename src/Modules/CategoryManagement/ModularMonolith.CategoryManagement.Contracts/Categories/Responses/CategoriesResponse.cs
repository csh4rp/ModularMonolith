using ModularMonolith.CategoryManagement.Contracts.Categories.Models;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Responses;

public sealed record CategoriesResponse(IReadOnlyList<CategoryItemModel> Items, int TotalLength)
    : IPaginatedResponse<CategoryItemModel>;
