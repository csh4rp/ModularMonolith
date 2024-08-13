using ModularMonolith.Identity.Contracts.Account.Registration;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Application.Account.Registration;

internal sealed class UserRegisteredEventMapping : IEventMapping<UserRegisteredEvent>
{
    public IntegrationEvent Map(UserRegisteredEvent @event) =>
        new UserRegisteredIntegrationEvent(@event.UserId, @event.Email)
        {
            EventId = @event.EventId, Timestamp = @event.Timestamp
        };
}
