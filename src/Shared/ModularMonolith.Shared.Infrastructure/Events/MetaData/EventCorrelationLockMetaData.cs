namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public class EventCorrelationLockMetaData
{
    public required string TableName { get; init; }

    public required string CorrelationIdColumnName { get; init; }
    
    public required string AcquiredAtColumnName { get; init; }
}
