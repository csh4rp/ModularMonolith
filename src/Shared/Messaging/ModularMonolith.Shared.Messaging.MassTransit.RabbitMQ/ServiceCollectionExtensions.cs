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
        OutboxStorageType outboxStorageType,
        Assembly[] assemblies)
        where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("RabbitMQ");

        var setupConsumers = configuration.GetSection("Messaging:CustomersEnabled")
            .Get<bool>();

        serviceCollection.AddMassTransit(c =>
        {
            c.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                switch (outboxStorageType)
                {
                    case OutboxStorageType.Postgres:
                        o.UsePostgres();
                        break;
                    case OutboxStorageType.SqlServer:
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

            c.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(connectionString);

                if (setupConsumers)
                {
                    SetupConsumers<TDbContext>(context, cfg, assemblies);
                }

                cfg.ConfigureEndpoints(context);
            });
        });

        return serviceCollection;
    }

    private static void SetupConsumers<TDbContext>(IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator cfg,
        Assembly[] assemblies)
        where TDbContext : DbContext
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
