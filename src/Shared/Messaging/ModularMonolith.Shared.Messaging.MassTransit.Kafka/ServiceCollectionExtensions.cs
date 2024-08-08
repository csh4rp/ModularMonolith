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
        OutboxStorageType outboxStorageType,
        Assembly[] assemblies) where TDbContext : DbContext
    {
        var options = configuration.GetSection("Kafka")
                          .Get<KafkaOptions>()
            ?? throw new NullReferenceException("Kafka section is missing");

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

                o.UseBusOutbox(a =>
                {
                    a.MessageDeliveryLimit = 10;
                });
            });

            c.AddConsumers(assemblies);

            c.AddConfigureEndpointsCallback((context, _, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<TDbContext>(context);
            });

            c.AddRider(cfg=>
            {
                cfg.SetDefaultEndpointNameFormatter();

                cfg.UsingKafka((context, configurator) =>
                {
                    configurator.Host(options.Host, host =>
                    {
                        host.UseSasl(saslConfigurator =>
                        {
                            saslConfigurator.Username = options.Username;
                            saslConfigurator.Password = options.Password;
                            saslConfigurator.Mechanism = SaslMechanism.Plain;
                        });

                        host.UseSsl(s => s.EndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https);
                    });

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
                            configurator.TopicEndpoint<object>(topic, consumerGroupName, cf =>
                            {
                                cf.UseEntityFrameworkOutbox<TDbContext>(context);

                                foreach (var consumerType in consumerTypes)
                                {
                                    cf.ConfigureConsumer(context, consumerType);
                                }
                            });
                        }
                    }
                });
            });

        });

        return serviceCollection;
    }
}
