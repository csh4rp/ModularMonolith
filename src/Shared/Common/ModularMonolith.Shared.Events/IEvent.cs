namespace ModularMonolith.Shared.Events;

public interface IEvent
{
    Guid EventId { get; }

    DateTimeOffset Timestamp { get; }
}
