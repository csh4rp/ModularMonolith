using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Modules.Identity.Domain.Users.Events;

public record UserCreated(Guid UserId, string Email) : IEvent;

