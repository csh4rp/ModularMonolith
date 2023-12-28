using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Common.Events;

public sealed record PasswordResetInitialized(Guid UserId) : IEvent;
