namespace ModularMonolith.Shared.DataAccess.AudiLogs;

public sealed record EntityFieldChange(string Name, object? OriginalValue, object? CurrentValue);
