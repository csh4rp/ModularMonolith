using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users;

public record SignInSucceededEvent(Guid UserId) : IEvent;
