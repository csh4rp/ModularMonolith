using MassTransit;

namespace ModularMonolith.Identity.Infrastructure.Account.Messaging;

public sealed class AnonymiseAccountsJobConsumerDefinition : ConsumerDefinition<AnonymiseAccountsJobConsumer>
{
    public AnonymiseAccountsJobConsumerDefinition()
    {
        EndpointName = "AnonymiseAccountsJobConsumer";
    }
}
