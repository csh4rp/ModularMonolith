using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Domain.Users;

[Event("AccountVerified")]
public sealed record AccountVerifiedEvent(Guid UserId) : DomainEvent;
