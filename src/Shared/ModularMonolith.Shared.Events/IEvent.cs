namespace ModularMonolith.Shared.Events;

public interface IEvent
{
    DateTimeOffset OccurredAt { get; }
}
