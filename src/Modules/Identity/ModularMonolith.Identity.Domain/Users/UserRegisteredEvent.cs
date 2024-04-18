using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users;

public sealed record UserRegisteredEvent(Guid UserId, string Email) : DomainEvent;
