using MassTransit;

namespace ModularMonolith.Identity.Infrastructure.Account.Messaging;

public class AnonymiseAccountsJobConsumerDefinition : ConsumerDefinition<AnonymiseAccountsJobConsumer>
{
    public AnonymiseAccountsJobConsumerDefinition()
    {
        // EndpointName = "AnonymiseAccountsJobConsumer";
    }
}
