using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging.MassTransit.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        DatabaseProvider databaseProvider,
        Assembly[] assemblies)
        where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("RabbitMQ");

        var setupConsumers = configuration.GetSection("Messaging:CustomersEnabled")
            .Get<bool>();


        serviceCollection
            .AddScoped<IMessageBus, MessageBus>()
            .AddMassTransit(c =>
        {
            c.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                switch (databaseProvider)
                {
                    case DatabaseProvider.Postgres:
                        o.UsePostgres();
                        break;
                    case DatabaseProvider.SqlServer:
                        o.UseSqlServer();
                        break;
                }

                o.UseBusOutbox(cfg =>
                {
                    cfg.MessageDeliveryLimit = 10;
                });
            });

            if (setupConsumers)
            {
                c.AddConsumers(assemblies);
            }

            c.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(connectionString);

                if (setupConsumers)
                {
                    SetupConsumers(context, configurator, assemblies);
                }

                configurator.ConfigureEndpoints(context);
            });
        });

        return serviceCollection;
    }

    private static void SetupConsumers(IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator cfg,
        Assembly[] assemblies)
    {
        var consumerMessages = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(typeof(IConsumer)))
            .GroupBy(t =>
            {
                var @interface = t.GetInterfaces()
                    .Single(i => i.IsGenericType && i.IsAssignableTo(typeof(IConsumer)));

                return @interface.GenericTypeArguments[0];
            })
            .ToDictionary(t => t.Key, t => t.ToList());

        foreach (var (messageType, _) in consumerMessages)
        {
            var eventAttribute = messageType.GetCustomAttribute<EventAttribute>()!;
            var topic = eventAttribute.Topic ?? messageType.Name;

            var groupedConsumers = consumerMessages.Values.SelectMany(s => s)
                .GroupBy(t => t.GetCustomAttribute<EventConsumerAttribute>()?.ConsumerName ?? topic)
                .ToDictionary(t => t.Key, t => t.ToList());

            foreach (var (queue, consumerTypes) in groupedConsumers)
            {
                cfg.ReceiveEndpoint(queue, cf =>
                {
                    foreach (var consumerType in consumerTypes)
                    {
                        cf.ConfigureConsumer(context, consumerType);
                    }

                    cf.Bind(topic);
                });
            }
        }
    }
}
