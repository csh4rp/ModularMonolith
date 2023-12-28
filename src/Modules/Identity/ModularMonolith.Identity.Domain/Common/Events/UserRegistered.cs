using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Common.Events;

public record UserRegistered(Guid UserId, string Email) : IEvent;
