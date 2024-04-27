namespace ModularMonolith.Shared.DataAccess.AudiLog;

public sealed record EntityFieldChange(string Name, object? OriginalValue, object? CurrentValue);
