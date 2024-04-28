using System.Diagnostics;
using System.Net;

namespace ModularMonolith.Shared.Tracing;

public interface IOperationContext
{
    string Subject { get; }

    string OperationName { get; }

    Uri? Uri { get; }

    IPAddress? IpAddress { get; }

    ActivityTraceId TraceId { get; }

    ActivitySpanId SpanId { get; }

    ActivitySpanId? ParentSpanId { get; }
}
