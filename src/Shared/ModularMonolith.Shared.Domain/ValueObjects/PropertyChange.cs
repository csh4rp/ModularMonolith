namespace ModularMonolith.Shared.Domain.ValueObjects;

public record PropertyChange(string PropertyName, object? CurrentValue, object? OriginalValue);
