namespace ModularMonolith.Shared.DataAccess.AudiLogs;

public sealed record EntityFieldChange(string Name, string? OriginalValue, string? CurrentValue);
