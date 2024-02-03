using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.CategoryManagement.Domain.Categories;

public class Category : Entity<CategoryId>, IAggregateRoot
{
    private Category()
    {
    }

    private Category(CategoryId id, CategoryId? parentId, string name)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        
        AddEvent(new CategoryCreated(id, parentId, name));
    }
    
    public CategoryId? ParentId { get; private set; }

    public string Name { get; private set; } = default!;

    public void Update(CategoryId? parentId,
        string name)
    {
        var hasChanges = HasChanges(parentId, name);

        if (!hasChanges)
        {
            return;
        }

        ParentId = parentId;
        Name = name;
        
        AddEvent(new CategoryUpdated(Id, ParentId, Name));
    }

    private bool HasChanges(CategoryId? parentId, string name) =>
        ParentId != parentId
        || Name != name;

    public static Category Create(string name, CategoryId? parentId) => new(new CategoryId(), parentId, name);

    public static Category From(CategoryId id, string name, CategoryId? parentId) => new()
    {
        Id = id, Name = name, ParentId = parentId
    };
}
