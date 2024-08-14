using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

public sealed class Category : AggregateRoot<CategoryId>
{
    private Category()
    {
    }

    public CategoryId? ParentId { get; private set; }

    public string Name { get; private set; } = default!;

    public void Update(CategoryId? parentId, string name)
    {
        var hasChanged = HasChanged(parentId, name);
        if (!hasChanged)
        {
            return;
        }

        ParentId = parentId;
        Name = name;

        EnqueueEvent(new CategoryUpdatedEvent(Id, ParentId, Name));
    }

    private bool HasChanged(CategoryId? parentId, string name) => ParentId != parentId || Name != name;

    public static Category From(CategoryId id, string name, CategoryId? parentId) => new()
    {
        Id = id,
        Name = name,
        ParentId = parentId
    };

    public static Category Create(CategoryId? parentId, string name)
    {
        var id = CategoryId.NewId();
        var category = new Category { Id = id, Name = name, ParentId = parentId };
        category.EnqueueEvent(new CategoryCreatedEvent(id, parentId, name));

        return category;
    }
}
