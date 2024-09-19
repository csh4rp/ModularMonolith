using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Domain.Users;

[Event("PasswordChanged")]
public sealed record PasswordResetInitializedEvent(Guid UserId) : DomainEvent;
