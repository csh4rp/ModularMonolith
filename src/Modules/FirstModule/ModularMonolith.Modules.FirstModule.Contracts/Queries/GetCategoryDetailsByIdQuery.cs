using ModularMonolith.Modules.FirstModule.Contracts.Responses;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.FirstModule.Contracts.Queries;

public class GetCategoryDetailsByIdQuery(Guid id) : IQuery<CategoryDetailsResponse?>
{
    public Guid Id { get; } = id;
}
