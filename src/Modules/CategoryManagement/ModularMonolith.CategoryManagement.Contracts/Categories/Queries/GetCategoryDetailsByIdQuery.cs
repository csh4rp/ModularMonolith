using ModularMonolith.CategoryManagement.Contracts.Categories.Responses;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Queries;

public class GetCategoryDetailsByIdQuery(Guid id) : IQuery<CategoryDetailsResponse?>
{
    public Guid Id { get; } = id;
}
