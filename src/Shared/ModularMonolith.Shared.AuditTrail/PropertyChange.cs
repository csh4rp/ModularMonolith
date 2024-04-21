namespace ModularMonolith.Shared.AuditTrail;

public record PropertyChange
{
    public PropertyChange()
    {
    }

    public PropertyChange(string propertyName, object? currentValue, object? originalValue)
    {
        PropertyName = propertyName;
        CurrentValue = currentValue?.ToString();
        OriginalValue = originalValue?.ToString();
    }

    public string PropertyName { get; private set; } = default!;

    public string? CurrentValue { get; private set; }

    public string? OriginalValue { get; private set; }
}
