using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.CategoryManagement.Infrastructure.Categories.Repositories;

namespace ModularMonolith.CategoryManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICategoryRepository, CategoryRepository>();

        return serviceCollection;
    }

    public static IRabbitMqBusFactoryConfigurator AddConsumerConfigurations(this IRabbitMqBusFactoryConfigurator bus)
    {
        bus.ReceiveEndpoint("", e =>
        {

        });

        return bus;
    }
}
