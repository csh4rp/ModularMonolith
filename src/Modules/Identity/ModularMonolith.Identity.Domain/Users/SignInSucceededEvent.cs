using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users;

public sealed record SignInSucceededEvent(Guid UserId) : DomainEvent;
