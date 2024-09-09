using MassTransit;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.Messaging;

public class CategoryCreatedConsumerDefinition : ConsumerDefinition<CategoryCreatedConsumer>
{
    public CategoryCreatedConsumerDefinition()
    {
        EndpointName = "CategoryCreated";
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<CategoryCreatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator, context);
    }
}
