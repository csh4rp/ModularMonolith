using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users;

public record PasswordChangedEvent(Guid UserId) : IEvent;
