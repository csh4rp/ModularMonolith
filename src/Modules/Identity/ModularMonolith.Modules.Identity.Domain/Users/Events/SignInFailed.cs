using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Modules.Identity.Domain.Users.Events;

public record SignInFailed(Guid UserId) : IEvent;
