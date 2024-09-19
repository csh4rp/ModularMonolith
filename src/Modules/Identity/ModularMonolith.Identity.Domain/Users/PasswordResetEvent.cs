using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Domain.Users;

[Event("PasswordReset")]
public sealed record PasswordResetEvent(Guid UserId) : DomainEvent;
