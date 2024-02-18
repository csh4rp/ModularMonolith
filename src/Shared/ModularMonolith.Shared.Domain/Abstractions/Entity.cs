namespace ModularMonolith.Shared.Domain.Abstractions;

public interface IEntity
{
    IEnumerable<IEvent> DequeueEvents();
}


public abstract class Entity<TId> : IEntity where TId : notnull
{
    private Queue<IEvent>? _events;

    public TId Id { get; init; } = default!;

    public int Version { get; private set; }

    protected void EnqueueEvent(IEvent @event)
    {
        if (_events is null)
        {
            _events = [];
            Version++;
        }

        _events.Enqueue(@event);
    }

    public IEnumerable<IEvent> DequeueEvents()
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
