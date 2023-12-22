using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Modules.Identity.Domain.Users.Events;

public record AccountVerified(Guid UserId) : IEvent;
