namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public record PropertyChange(object? CurrentValue, object? OriginalValue);
