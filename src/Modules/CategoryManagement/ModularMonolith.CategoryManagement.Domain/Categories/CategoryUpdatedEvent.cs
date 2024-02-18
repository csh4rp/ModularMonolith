using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

[Event(nameof(CategoryUpdatedEvent), Topic = nameof(Category), IsPersisted = true)]
public sealed record CategoryUpdatedEvent(CategoryId Id, CategoryId? ParentId, string Name) : IEvent;
