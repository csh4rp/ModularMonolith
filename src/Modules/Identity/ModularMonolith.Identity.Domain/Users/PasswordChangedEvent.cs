using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Domain.Users;

[Event("PasswordChanged")]
public record PasswordChangedEvent(Guid UserId) : DomainEvent;
