namespace ModularMonolith.Shared.Domain.Abstractions;

public abstract class Entity<TId> : IEntity where TId : IEquatable<TId>
{
    private Queue<DomainEvent>? _events;

    public TId Id { get; init; } = default!;

    public int Version { get; private set; }

    protected virtual void EnqueueEvent(DomainEvent @event)
    {
        if (_events is null)
        {
            _events = [];
            Version++;
        }

        _events.Enqueue(@event);
    }

    public IEnumerable<DomainEvent> DequeueEvents()
    {
        if (_events is null)
        {
            yield break;
        }

        while (_events.TryDequeue(out var @event))
        {
            yield return @event;
        }
    }
}
