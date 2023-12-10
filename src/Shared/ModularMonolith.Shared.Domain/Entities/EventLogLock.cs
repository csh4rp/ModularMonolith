namespace ModularMonolith.Shared.Domain.Entities;

public class EventLogLock
{
    public Guid CorrelationId { get; init; }
    
    public DateTimeOffset EndsAt { get; init; }
}
