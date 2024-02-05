using System.Text.Json.Serialization;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Querying;

public sealed record GetCategoryDetailsByIdQuery(Guid Id) : IQuery<CategoryDetailsResponse>
{
    [JsonIgnore]
    public Guid Id { get; } = Id;
}
