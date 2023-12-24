using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users.Events;

public record SignInSucceeded(Guid UserId) : IEvent;
