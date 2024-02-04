using System.Text.Json.Serialization;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Deletion;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand
{
    [JsonIgnore]
    public Guid Id { get; } = Id;
}
