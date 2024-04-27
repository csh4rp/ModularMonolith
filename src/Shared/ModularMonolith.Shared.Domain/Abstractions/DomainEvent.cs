using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Domain.Abstractions;

public abstract record DomainEvent : IEvent
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset Timestamp { get; protected set; } = TimeProvider.System.GetUtcNow();
}
