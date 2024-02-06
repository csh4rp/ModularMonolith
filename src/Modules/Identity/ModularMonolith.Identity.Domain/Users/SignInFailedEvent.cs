using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users;

public record SignInFailedEvent(Guid UserId) : IEvent;
