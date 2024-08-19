using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Domain.Users;

[Event("SignInFailed")]
public sealed record SignInFailedEvent(Guid UserId) : DomainEvent;
