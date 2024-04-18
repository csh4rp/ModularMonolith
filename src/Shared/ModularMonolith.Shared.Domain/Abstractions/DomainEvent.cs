using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Domain.Abstractions;

public abstract record DomainEvent : IEvent
{
    public DateTimeOffset OccurredAt { get; protected set; } = DateTimeOffset.UtcNow;
}
