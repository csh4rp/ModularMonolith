using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.CategoryManagement.Infrastructure;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Infrastructure.Messaging.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("RabbitMQ");
        var provider = configuration.GetSection("DataAccess:Provider").Value;

        serviceCollection
            .AddScoped<IMessageBus, MessageBus>()
            .AddMassTransit(c =>
            {
                c.AddEntityFrameworkOutbox<TDbContext>(o =>
                {
                    switch (provider)
                    {
                        case "Postgres":
                            o.UsePostgres();
                            break;
                        case "SqlServer":
                            o.UseSqlServer();
                            break;
                    }

                    o.UseBusOutbox(cfg =>
                    {
                        cfg.MessageDeliveryLimit = 10;
                    });
                });

                c.AddConsumers(consumerAssemblies);

                c.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(connectionString);
                    configurator.ConfigureEndpoints(context);

                    configurator.AddCategoryManagementConsumerConfigurations();
                });
            });

        return serviceCollection;
    }
}
