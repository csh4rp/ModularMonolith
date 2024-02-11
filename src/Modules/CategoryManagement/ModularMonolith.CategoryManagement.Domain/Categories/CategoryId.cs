namespace ModularMonolith.CategoryManagement.Domain.Categories;

public readonly record struct CategoryId
{
    public CategoryId() => Value = Guid.NewGuid();

    public CategoryId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value can't be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; init; }
}
