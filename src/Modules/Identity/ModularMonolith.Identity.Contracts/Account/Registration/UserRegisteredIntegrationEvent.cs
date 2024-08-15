using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Registration;

public sealed record UserRegisteredIntegrationEvent(Guid UserId, string Email)
    : IntegrationEvent;
