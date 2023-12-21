namespace ModularMonolith.Shared.Domain.Entities;

public class EventCorrelationLock
{
    public Guid CorrelationId { get; init; }

    public DateTimeOffset AcquiredAt { get; init; }
}
