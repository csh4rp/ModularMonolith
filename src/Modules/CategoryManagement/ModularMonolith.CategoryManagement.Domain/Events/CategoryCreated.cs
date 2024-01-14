using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.CategoryManagement.Domain.Events;

[Event(nameof(CategoryCreated), "categories", IsPersisted = true)]
public record CategoryCreated(Guid Id, string Name) : IEvent
{
    public CategoryCreated() : this(default!, default!)
    { 
    }
    
    public Guid Id { get; set; } = Id;
    
    public string Name { get; set; } = Name;
}
