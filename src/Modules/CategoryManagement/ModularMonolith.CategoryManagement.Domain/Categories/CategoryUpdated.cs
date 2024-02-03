using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

public record CategoryUpdated(CategoryId Id, CategoryId? ParentId, string Name) : IEvent;

