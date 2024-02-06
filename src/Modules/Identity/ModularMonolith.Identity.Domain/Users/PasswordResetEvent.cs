using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users;

public record PasswordResetEvent(Guid UserId) : IEvent;
