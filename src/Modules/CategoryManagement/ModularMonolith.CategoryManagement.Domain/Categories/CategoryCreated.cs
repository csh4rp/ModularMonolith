using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

[Event(nameof(CategoryCreated), "categories", IsPersisted = true)]
public record CategoryCreated(CategoryId Id, CategoryId? ParentId, string Name) : IEvent
{
    public CategoryCreated() : this(default!, default, default!)
    { 
    }
    
    public CategoryId Id { get; set; } = Id;
    
    public CategoryId? ParentId { get; set; } = ParentId;
    
    public string Name { get; set; } = Name;
}
