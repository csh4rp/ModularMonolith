using System.Text.Json.Serialization;
using ModularMonolith.CategoryManagement.Contracts.Categories.Responses;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Queries;

public sealed record GetCategoryDetailsByIdQuery(Guid Id) : IQuery<CategoryDetailsResponse?>
{
    [JsonIgnore]
    public Guid Id { get; } = Id;
}
