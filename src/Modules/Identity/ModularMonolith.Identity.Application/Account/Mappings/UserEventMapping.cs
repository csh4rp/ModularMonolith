using ModularMonolith.Identity.Contracts.Account.Events;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Application.Account.Mappings;

internal sealed class UserEventMapping : IEventMapping<UserRegistered>
{
    public IIntegrationEvent Map(UserRegistered @event) =>
        new UserRegisteredIntegrationEvent(@event.UserId, @event.Email);
}
