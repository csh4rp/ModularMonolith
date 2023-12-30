using System.Text.Json.Serialization;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Commands;

public record UpdateCategoryCommand(Guid Id, Guid? ParentId, string Name) : ICommand
{
    [JsonIgnore]
    public Guid Id { get; set; } = Id;

    public Guid? ParentId { get; private set; } = ParentId;

    public string Name { get; private set; } = Name;
}
