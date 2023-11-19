using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Abstract;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;
using ModularMonolith.Shared.Infrastructure.Messaging.Filters;
using ModularMonolith.Shared.Infrastructure.Messaging.Options;
using ModularMonolith.Shared.Infrastructure.Messaging.Utils;
using RabbitMQ.Client;

namespace ModularMonolith.Shared.Infrastructure.Messaging.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection serviceCollection, Action<MessagingOptions> configurationAction)
    {
        serviceCollection.AddOptions<MessagingOptions>()
            .Configure(configurationAction);

        var options = new MessagingOptions();
        configurationAction.Invoke(options);
        
        serviceCollection.AddMassTransit(configurator =>
        {
            var types = options.Assemblies.SelectMany(a => a.GetTypes()).ToArray();
            var allConsumerTypes = types.Where(t => t.IsAssignableTo(typeof(IConsumer))).ToArray();

            foreach (var consumerType in allConsumerTypes)
            {
                var consumerDefinitionType = types.FirstOrDefault(t =>
                    t.IsAssignableTo(typeof(ConsumerDefinition<>).MakeGenericType(consumerType)));

                configurator.AddConsumer(consumerType, consumerDefinitionType);
            }
            
            configurator.UsingRabbitMq((cnx, cfg) =>
            {
                cfg.Host(options.HostUri, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
                
                cfg.MessageTopology.SetEntityNameFormatter(new AttributeEntityNameFormatter());
                
                ConfigureMessages(cfg, types);
                
                ConfigureEndpoints(allConsumerTypes, cfg, cnx);
            });
        });
        
        return serviceCollection;
    }

    private static void ConfigureEndpoints(Type[] allConsumerTypes,
        IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext cnx)
    {
        var consumers = GetConsumers(allConsumerTypes);
        
        foreach (var (queue, consumerTypes) in consumers)
        {
            cfg.ReceiveEndpoint(queue, endpoint =>
            {
                foreach (var consumerType in consumerTypes)
                {
                    endpoint.ConfigureConsumer(cnx, consumerType);
                }

                var topics = consumerTypes.Select(c =>
                {
                    var messageType = c.GenericTypeArguments[0];
                    var eventAttribute = messageType.GetCustomAttribute<EventAttribute>();

                    return eventAttribute?.Topic ?? messageType.Name;
                }).ToHashSet();

                foreach (var topic in topics)
                {
                    endpoint.Bind(topic);
                }

                endpoint.UseConsumeFilter(typeof(TransactionEnlistingEventFilter<>), cnx);
            });
        }
    }

    private static Dictionary<string, List<Type>> GetConsumers(Type[] allConsumerTypes)
    {
        var consumers = allConsumerTypes
            .GroupBy(t =>
            {
                var queue = t.GetCustomAttribute<EventConsumerAttribute>()?.Queue;

                if (!string.IsNullOrEmpty(queue))
                {
                    return queue;
                }

                var messageType = t.GenericTypeArguments[0];
                var eventAttribute = messageType.GetCustomAttribute<EventAttribute>();

                return eventAttribute?.Topic ?? messageType.Name;
            }).ToDictionary(k => k.Key, v => v.ToList());
        return consumers;
    }

    private static void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg, Type[] types)
    {
        var messageTypes = types.Where(t =>
            t.IsAssignableTo(typeof(IEvent)) || t.IsAssignableTo(typeof(IIntegrationEvent)));

        foreach (var messageType in messageTypes)
        {
            cfg.Publish(messageType, msg =>
            {
                msg.ExchangeType = ExchangeType.Fanout;
                msg.Durable = true;
            });
        }
    }
}
