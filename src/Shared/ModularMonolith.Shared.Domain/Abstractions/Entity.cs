namespace ModularMonolith.Shared.Domain.Abstractions;

public abstract class Entity<TId> where TId : notnull
{
    private List<IEvent>? _events;
    
    public TId Id { get; init; } = default!;
    public int Version { get; private set; }

    protected void AddEvent(IEvent @event)
    {
        if (_events is null)
        {
            _events = [];
            Version++;
        }
        
        _events.Add(@event);
        
    }

    public IEnumerable<IEvent> DequeueEvents()
    {
        if (_events is null)
        {
            return Enumerable.Empty<IEvent>();
        }

        var events = _events.ToList();
        _events.Clear();
        
        return events;
    }
}
