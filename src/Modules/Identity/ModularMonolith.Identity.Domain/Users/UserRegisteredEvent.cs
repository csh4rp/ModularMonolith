using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Common.Events;

public record UserRegisteredEvent(Guid UserId, string Email) : IEvent;
