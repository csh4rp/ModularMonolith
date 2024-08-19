using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Identity.Domain.Users;

[Event("SignInSucceeded")]
public sealed record SignInSucceededEvent(Guid UserId) : DomainEvent;
