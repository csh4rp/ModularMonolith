using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

[Event(nameof(CategoryCreatedEvent), Topic = nameof(Category), IsPersisted = true)]
public sealed record CategoryCreatedEvent(CategoryId Id, CategoryId? ParentId, string Name) : DomainEvent
{
    public CategoryCreatedEvent() : this(default!, default, default!)
    {
    }

    public CategoryId Id { get; set; } = Id;

    public CategoryId? ParentId { get; set; } = ParentId;

    public string Name { get; set; } = Name;
}
