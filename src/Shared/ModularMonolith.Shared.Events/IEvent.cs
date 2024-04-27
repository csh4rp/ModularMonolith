namespace ModularMonolith.Shared.Events;

public interface IEvent
{
    Guid Id { get; }

    DateTimeOffset Timestamp { get; }
}
