using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.CategoryManagement.Domain.Events;

public record CategoryCreated(Guid Id, string Name) : IEvent
{
    public CategoryCreated() : this(default!, default!)
    { 
    }
    
    public Guid Id { get; set; } = Id;
    
    public string Name { get; set; } = Name;
}
