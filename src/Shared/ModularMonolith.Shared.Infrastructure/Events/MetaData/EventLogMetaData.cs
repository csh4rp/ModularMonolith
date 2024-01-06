namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

internal sealed class EventLogMetaData
{
    public required string TableName { get; init; }

    public required string IdColumnName { get; init; }

    public required string CorrelationIdColumnName { get; init; }

    public required string EventNameColumnName { get; init; }

    public required string EventPayloadColumnName { get; init; }

    public required string EventTypeColumnName { get; init; }

    public required string TraceIdColumnName { get; init; }

    public required string SpanIdColumnName { get; init; }

    public required string ParentSpanIdColumnName { get; init; }

    public required string CreatedAtColumnName { get; init; }

    public required string OperationNameColumnName { get; init; }

    public required string UserIdColumnName { get; init; }

    public required string UserNameColumnName { get; init; }

    public required string PublishedAtColumnName { get; init; }

    public required string IpAddressColumnName { get; init; }

    public required string UserAgentColumnName { get; init; }
    
    public required string TopicColumnName { get; init; }
}
