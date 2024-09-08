using MassTransit;

namespace ModularMonolith.CategoryManagement.Messaging.Categories;

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
