using MassTransit;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.MessageConfigurations;

public class CategoryMessagingConfiguration
{
    public void Apply(IRabbitMqBusFactoryConfigurator configurator)
    {
        // configurator.Message<CategoryCreatedEvent>(e => e.SetEntityName("categories"));
        //
        // configurator.ReceiveEndpoint("categories", e =>
        // {
        //     e.Bind<CategoryCreatedEvent>();
        // });
    }
}
