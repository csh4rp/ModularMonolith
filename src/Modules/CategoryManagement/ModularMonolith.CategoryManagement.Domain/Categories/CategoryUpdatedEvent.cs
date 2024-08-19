using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

[Event(nameof(CategoryUpdatedEvent), IsPersisted = true)]
public sealed record CategoryUpdatedEvent(CategoryId CategoryId, CategoryId? ParentId, string Name) : DomainEvent;
