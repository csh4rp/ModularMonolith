using System.Diagnostics;

namespace ModularMonolith.Shared.DataAccess.AudiLogs;

public record AuditMetaData
{
    public required string? Subject { get; init; }

    public required string? OperationName { get; init; }

    public required ActivityTraceId? TraceId { get; init; }

    public required ActivitySpanId? SpanId { get; init; }

    public required ActivitySpanId? ParentSpanId { get; init; }

    public required IReadOnlyDictionary<string, string?> ExtraData { get; init; }
}
