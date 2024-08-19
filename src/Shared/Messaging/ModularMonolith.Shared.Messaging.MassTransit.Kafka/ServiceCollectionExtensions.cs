using System.Reflection;
using Confluent.Kafka;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging.MassTransit.Kafka;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        DatabaseProvider databaseProvider,
        Assembly[] assemblies) where TDbContext : DbContext
    {
        var options = configuration.GetSection("Kafka")
                          .Get<KafkaOptions>()
                      ?? throw new NullReferenceException("Kafka section is missing");

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

                o.UseBusOutbox(a =>
                {
                    a.MessageDeliveryLimit = 10;
                });
            });

            if (setupConsumers)
            {
                c.AddConsumers(assemblies);
            }

            c.UsingInMemory();

            c.AddRider(cfg =>
            {
                cfg.SetDefaultEndpointNameFormatter();

                cfg.UsingKafka((context, configurator) =>
                {
                    configurator.Host(options.Host, host =>
                    {
                        if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
                        {
                            host.UseSasl(saslConfigurator =>
                            {
                                saslConfigurator.Username = options.Username;
                                saslConfigurator.Password = options.Password;
                                saslConfigurator.Mechanism = SaslMechanism.Plain;
                            });

                            host.UseSsl(s =>
                                s.EndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https);
                        }
                    });

                    if (setupConsumers)
                    {
                        SetupConsumers(context, configurator, assemblies);
                    }
                });
            });
        });

        return serviceCollection;
    }

    private static void SetupConsumers(IRiderRegistrationContext context,
        IKafkaFactoryConfigurator cfg,
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

            foreach (var (consumerGroupName, consumerTypes) in groupedConsumers)
            {
                cfg.topicen
                cfg.TopicEndpoint<string, object>(topic, consumerGroupName, cf =>
                {
                    cf.SetValueDeserializer(new Avro);
                    foreach (var consumerType in consumerTypes)
                    {
                        cf.ConfigureConsumer(context, consumerType);
                    }
                });
            }
        }
    }
}
