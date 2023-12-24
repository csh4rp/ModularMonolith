using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users.Events;

public record PasswordChanged(Guid UserId) : IEvent;
