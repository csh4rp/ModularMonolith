using ModularMonolith.Modules.FirstModule.Contracts.Categories.Responses;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.FirstModule.Contracts.Categories.Queries;

public class GetCategoryDetailsByIdQuery(Guid id) : IQuery<CategoryDetailsResponse?>
{
    public Guid Id { get; } = id;
}
