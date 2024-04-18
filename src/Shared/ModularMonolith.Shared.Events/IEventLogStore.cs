namespace ModularMonolith.Shared.Events;

public interface IEventLogStore
{
    Task<TEvent?> FindFirstOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent;

    Task<EventLog?> FindFirstOccurenceAsync(string subject, Type eventType, CancellationToken cancellationToken);

    Task<TEvent?> FindLastOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken) where TEvent : IEvent;

    Task<EventLog?> FindLastOccurenceAsync(string subject, Type eventType, CancellationToken cancellationToken);
}
