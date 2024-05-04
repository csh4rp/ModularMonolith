namespace ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.EventLogs.Models;

public class EventLogEntityMetaData
{
    public required string? Subject { get; init; }

    public required string? IpAddress { get; init; }

    public required string? Uri { get; init; }

    public required string? OperationName { get; init; }

    public required string? TraceId { get; init; }

    public required string? SpanId { get; init; }

    public required string? ParentSpanId { get; init; }
}
