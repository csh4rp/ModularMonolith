namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

internal sealed class EventLogMetaData
{
    public required string TableName { get; init; }

    public required string IdColumnName { get; init; }

    public required string CorrelationIdColumnName { get; init; }

    public required string NameColumnName { get; init; }

    public required string PayloadColumnName { get; init; }

    public required string TypeColumnName { get; init; }

    public required string ActivityIdColumnName { get; init; }

    public required string CreatedAtColumnName { get; init; }

    public required string OperationNameColumnName { get; init; }

    public required string UserIdColumnName { get; init; }

    public required string PublishedAtColumnName { get; init; }

    public required string IpAddressColumnName { get; init; }

    public required string UserAgentColumnName { get; init; }
}
