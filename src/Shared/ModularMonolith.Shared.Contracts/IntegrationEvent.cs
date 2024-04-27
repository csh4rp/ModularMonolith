using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Contracts;

public abstract record IntegrationEvent(Guid Id, DateTimeOffset Timestamp) : IEvent;
