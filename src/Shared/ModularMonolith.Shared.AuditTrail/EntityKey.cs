namespace ModularMonolith.Shared.AuditTrail;

public record EntityKey
{
    public EntityKey()
    {
    }

    public EntityKey(string propertyName, object? value)
    {
        PropertyName = propertyName;
        Value = value?.ToString();
    }

    public string PropertyName { get; private set; } = default!;

    public string? Value { get; private set; } = default!;
}
