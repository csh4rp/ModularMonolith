namespace ModularMonolith.CategoryManagement.Domain.Categories;

public readonly record struct CategoryId
{
    private CategoryId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value can't be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; init; }

    public static CategoryId NewId() => new(Guid.NewGuid());

    public static CategoryId From(Guid value) => new(value);

    public static implicit operator Guid(CategoryId categoryId) => categoryId.Value;
}
