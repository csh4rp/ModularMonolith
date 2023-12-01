using ModularMonolith.Modules.FirstModule.Contracts.Models;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.FirstModule.Contracts.Responses;

public class CategoriesResponse : IPaginatedResponse<CategoryItemModel>
{
    public required IReadOnlyList<CategoryItemModel> Items { get; init; }
    
    public required int TotalLength { get; init; }
}
