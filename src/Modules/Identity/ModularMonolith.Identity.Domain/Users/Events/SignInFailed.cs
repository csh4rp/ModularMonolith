using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users.Events;

public record SignInFailed(Guid UserId) : IEvent;
