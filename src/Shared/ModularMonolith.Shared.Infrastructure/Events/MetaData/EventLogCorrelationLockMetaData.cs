namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

internal sealed class EventLogCorrelationLockMetaData
{
    public required string TableName { get; init; }

    public required string CorrelationIdColumnName { get; init; }

    public required string AcquiredAtColumnName { get; init; }
}
