using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Registration;

public record UserRegisteredIntegrationEvent(Guid UserId, string Email) : IIntegrationEvent;
