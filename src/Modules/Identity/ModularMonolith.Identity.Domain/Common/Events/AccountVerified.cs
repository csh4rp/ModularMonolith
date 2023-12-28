using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Common.Events;

public record AccountVerified(Guid UserId) : IEvent;
