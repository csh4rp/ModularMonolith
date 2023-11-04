namespace ModularMonolith.Shared.Infrastructure.Events;

public class EventLog
{
    public long Id { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset? PublishedAt { get; init; }
    
    public string? UserId { get; init; }
    
    public required string Topic { get; init; }
    
    public required string Stream { get; init; }
    
    public required string Type { get; init; }
    
    public required string Payload { get; init; }
}
