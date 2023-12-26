using System.Text.Json.Serialization;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.Contracts.Categories.Commands;

public class UpdateCategoryCommand : ICommand
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public required Guid? ParentId { get; set; }

    public required string Name { get; set; }
}
