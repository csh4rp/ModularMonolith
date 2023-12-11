namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public class EventLogMetaData
{
    public required string TableName { get; init; }
    
    public required string IdColumnName { get; init; }
    
    public required string CorrelationIdColumnName { get; init; }
    
    public required string NameColumnName { get; set; }
    
    public required string PayloadColumnName { get; set; }
    public required string TypeColumnName { get; set; }
    public required string ActivityIdColumnName { get; set; }
    public required string CreatedAtColumnName { get; set; }
    public required string OperationNameColumnName { get; set; }
    public required string UserIdColumnName { get; set; }
    public required string PublishedAtColumnName { get; set; }
}
