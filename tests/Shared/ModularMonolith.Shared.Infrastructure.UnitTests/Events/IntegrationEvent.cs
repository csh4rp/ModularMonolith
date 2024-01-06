using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Infrastructure.UnitTests.Events;

public record IntegrationEvent(string Name) : IIntegrationEvent;
