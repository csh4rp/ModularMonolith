using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.Infrastructure.UnitTests.Events;

public record DomainEvent(string Name) : IEvent;
