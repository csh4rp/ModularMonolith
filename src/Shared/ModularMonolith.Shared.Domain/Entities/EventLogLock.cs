namespace ModularMonolith.Shared.Domain.Entities;

public class EventLogLock
{
    public Guid EventLogId { get; init; }

    public DateTimeOffset AcquiredAt { get; init; }
}
