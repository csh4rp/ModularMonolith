using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Contracts;

public abstract record IntegrationEvent : IEvent
{
    public required Guid EventId { get; init; } = Guid.NewGuid();

    public required DateTimeOffset Timestamp { get; init; } = TimeProvider.System.GetUtcNow();
}
