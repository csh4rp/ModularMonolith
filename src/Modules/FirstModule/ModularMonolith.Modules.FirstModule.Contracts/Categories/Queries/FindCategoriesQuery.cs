using ModularMonolith.Modules.FirstModule.Contracts.Categories.Responses;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.FirstModule.Contracts.Categories.Queries;

public sealed class FindCategoriesQuery : IQuery<CategoriesResponse>, IPaginatedQuery
{
    public required string? Search { get; init; }
    
    public required string? OrderBy { get; init; }
    
    public required int? Skip { get; init; }
    
    public required int? Take { get; init; }
}
