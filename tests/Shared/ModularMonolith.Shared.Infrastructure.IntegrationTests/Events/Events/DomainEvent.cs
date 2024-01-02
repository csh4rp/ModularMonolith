using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events.Events;

public record DomainEvent(string Name) : IEvent;
