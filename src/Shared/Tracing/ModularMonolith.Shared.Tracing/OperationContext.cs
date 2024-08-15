using System.Diagnostics;
using System.Net;

namespace ModularMonolith.Shared.Tracing;

internal sealed record OperationContext : IOperationContext
{
    public required string Subject { get; init; }
    public required string OperationName { get; init; }
    public required Uri? Uri { get; init; }
    public required IPAddress? IpAddress { get; init; }
    public required ActivityTraceId TraceId { get; init; }
    public required ActivitySpanId SpanId { get; init; }
    public required ActivitySpanId? ParentSpanId { get; init; }
}
