using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users.Events;

public record UserRegistered(Guid UserId, string Email) : IEvent;
