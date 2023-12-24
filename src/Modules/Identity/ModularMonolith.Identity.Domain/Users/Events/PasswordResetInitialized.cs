using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users.Events;

public sealed record PasswordResetInitialized(Guid UserId) : IEvent;
