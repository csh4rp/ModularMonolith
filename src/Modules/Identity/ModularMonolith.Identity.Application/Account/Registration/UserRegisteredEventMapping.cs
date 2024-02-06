using ModularMonolith.Identity.Contracts.Account.Registration;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Application.Account.Registration;

internal sealed class UserRegisteredEventMapping : IEventMapping<UserRegisteredEvent>
{
    public IIntegrationEvent Map(UserRegisteredEvent @event) =>
        new UserRegisteredIntegrationEvent(@event.UserId, @event.Email);
}
