using ModularMonolith.Modules.FirstModule.Contracts.Categories.Models;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.FirstModule.Contracts.Categories.Responses;

public class CategoriesResponse : IPaginatedResponse<CategoryItemModel>
{
    public required IReadOnlyList<CategoryItemModel> Items { get; init; }
    
    public required int TotalLength { get; init; }
}
