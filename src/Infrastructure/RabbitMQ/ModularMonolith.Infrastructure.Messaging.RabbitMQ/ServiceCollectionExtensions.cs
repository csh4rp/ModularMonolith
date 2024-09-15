using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;
using ModularMonolith.Shared.Messaging.MassTransit.Filters;

namespace ModularMonolith.Infrastructure.Messaging.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies,
        bool runConsumers) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("RabbitMQ")
                               ?? throw new NullReferenceException("RabbitMQ ConnectionString is required");
        var provider = configuration.GetSection("DataAccess").GetValue<string>("Provider");

        serviceCollection
            .AddScoped<IMessageBus, MessageBus>()
            .AddScoped<EventLogEntryFactory>()
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

                c.AddJobSagaStateMachines();
                c.AddDelayedMessageScheduler();

                if (runConsumers)
                {
                    c.AddConsumers(consumerAssemblies);
                }

                c.UsingRabbitMq((context, configurator) =>
                {
                    configurator.UseConsumeFilter(typeof(IdentityFilter<>), context);
                    configurator.Host(connectionString);
                    configurator.ConfigureEndpoints(context);
                    configurator.UseDelayedMessageScheduler();
                });
            });

        serviceCollection.AddHealthChecks()
            .AddRabbitMQ(new Uri(connectionString), tags: ["live", "ready"]);

        return serviceCollection;
    }
}
