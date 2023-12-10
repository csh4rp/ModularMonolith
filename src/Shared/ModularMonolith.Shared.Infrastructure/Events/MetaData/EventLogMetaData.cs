namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public class EventLogMetaData
{
    public required string TableName { get; init; }
    
    public required string IdColumnName { get; init; }
    
    public required string CorrelationIdColumnName { get; init; }
}
