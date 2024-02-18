using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

public sealed class Category : Entity<CategoryId>, IAggregateRoot
{
    private Category()
    {
    }

    public Category(CategoryId? parentId, string name)
    {
        Id = CategoryId.NewId();
        ParentId = parentId;
        Name = name;

        EnqueueEvent(new CategoryCreatedEvent(Id, parentId, name));
    }

    public CategoryId? ParentId { get; private set; }

    public string Name { get; private set; } = default!;

    public void Update(CategoryId? parentId, string name)
    {
        var hasChanges = HasChanges(parentId, name);
        if (!hasChanges)
        {
            return;
        }

        ParentId = parentId;
        Name = name;

        EnqueueEvent(new CategoryUpdatedEvent(Id, ParentId, Name));
    }

    private bool HasChanges(CategoryId? parentId, string name) => ParentId != parentId || Name != name;

    public static Category From(CategoryId id, string name, CategoryId? parentId) => new()
    {
        Id = id, Name = name, ParentId = parentId
    };
}
