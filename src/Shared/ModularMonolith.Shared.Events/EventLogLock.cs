namespace ModularMonolith.Shared.Events;

public class EventLogLock
{
    public Guid EventLogId { get; init; }

    public DateTimeOffset AcquiredAt { get; init; }
}
