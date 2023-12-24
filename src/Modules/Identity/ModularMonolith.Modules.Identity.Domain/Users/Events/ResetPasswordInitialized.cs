using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Modules.Identity.Domain.Users.Events;

public sealed record ResetPasswordInitialized(Guid UserId) : IEvent;
