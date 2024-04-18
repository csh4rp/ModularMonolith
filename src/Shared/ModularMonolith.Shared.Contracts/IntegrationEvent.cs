using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Contracts;

public abstract record IntegrationEvent(DateTimeOffset OccurredAt) : IEvent;
