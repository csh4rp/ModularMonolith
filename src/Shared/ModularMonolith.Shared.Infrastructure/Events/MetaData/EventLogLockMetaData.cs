namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

internal sealed class EventLogLockMetaData
{
    public required string TableName { get; init; }

    public required string IdColumnName { get; init; }
    
    public required string AcquiredAtColumnName { get; init; }
}
