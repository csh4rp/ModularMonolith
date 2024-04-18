using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Registration;

public sealed record UserRegisteredIntegrationEvent(DateTimeOffset OccurredAt, Guid UserId, string Email)
    : IntegrationEvent(OccurredAt);
