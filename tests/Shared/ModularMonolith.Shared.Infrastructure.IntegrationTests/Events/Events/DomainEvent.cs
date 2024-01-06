using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events.Events;

[Event("domain-event")]
public record DomainEvent(string Name) : IEvent;
