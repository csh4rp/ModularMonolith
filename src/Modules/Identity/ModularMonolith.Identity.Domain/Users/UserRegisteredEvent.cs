using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Domain.Users;

[Event("UserRegistered")]
public sealed record UserRegisteredEvent(Guid UserId, string Email) : DomainEvent;
