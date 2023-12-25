using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Events;

public record UserRegisteredIntegrationEvent(Guid UserId, string Email) : IIntegrationEvent;
