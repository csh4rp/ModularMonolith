namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public class EventLockMetaData
{
    public required string TableName { get; init; }

    public required string CorrelationIdColumnName { get; set; }
    
    public required string AcquiredAtColumnName { get; set; }
}
