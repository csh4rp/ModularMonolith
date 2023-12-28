using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Identity.Domain.Common.Events;

public record PasswordReset(Guid UserId) : IEvent;
