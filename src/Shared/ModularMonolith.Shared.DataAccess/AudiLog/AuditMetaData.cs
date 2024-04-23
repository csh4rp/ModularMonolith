using System.Net;

namespace ModularMonolith.Shared.DataAccess.AudiLog;

public record AuditMetaData
{
    public required string? Subject { get; init; }

    public required IPAddress? IpAddress { get; init; }

    public required Uri? Uri { get; init; }

    public required string? OperationName { get; init; }

    public required string? TraceId { get; init; }

    public required string? SpanId { get; init; }

    public required string? ParentSpanId { get; init; }
}
