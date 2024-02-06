using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Users;

public record AccountVerifiedEvent(Guid UserId) : IEvent;
