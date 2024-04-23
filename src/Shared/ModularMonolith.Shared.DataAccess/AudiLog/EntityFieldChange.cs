namespace ModularMonolith.Shared.DataAccess.AudiLog;

public record EntityFieldChange(string Name, object? OriginalValue, object? CurrentValue);
