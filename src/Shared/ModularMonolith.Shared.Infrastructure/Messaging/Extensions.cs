using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Abstract;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Attributes;
using RabbitMQ.Client;

namespace ModularMonolith.Shared.Infrastructure.Messaging;

public static class Extensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection serviceCollection, Action<MessagingOptions> configurationAction)
    {
        serviceCollection.AddOptions<MessagingOptions>()
            .Configure(configurationAction);

        var options = new MessagingOptions();
        configurationAction.Invoke(options);
        
        serviceCollection.AddMassTransit(configurator =>
        {
            configurator.AddConsumers(_ => true, options.Assemblies.ToArray());
            
            configurator.UsingRabbitMq((cnx, cfg) =>
            {
                cfg.Host(options.Uri);
                
                cfg.MessageTopology.SetEntityNameFormatter(new AttributeEntityNameFormatter());
                
                var consumerMessages = options.Assemblies.SelectMany(a => a.GetTypes())
                    .Where(t => t.IsAssignableTo(typeof(IConsumer)))
                    .GroupBy(t => t.GenericTypeArguments[0])
                    .ToDictionary(t => t.Key, t => t.ToList());
                
                foreach (var messageType in consumerMessages.Keys)
                {
                    cfg.Publish(messageType, msg =>
                    {
                        msg.ExchangeType = ExchangeType.Fanout;
                        msg.Durable = true;
                    });
                }

                foreach (var (messageType, _) in consumerMessages)
                {
                    var eventAttribute = messageType.GetCustomAttribute<EventAttribute>()!;
                    var topic = eventAttribute.Topic ?? messageType.Name;

                    var groupedConsumers = consumerMessages.Values.SelectMany(s => s)
                        .GroupBy(t => t.GetCustomAttribute<EventConsumerAttribute>()?.Queue ?? topic)
                        .ToDictionary(t => t.Key, t => t.ToList());

                    foreach (var (queue, consumerTypes) in groupedConsumers)
                    {
                        cfg.ReceiveEndpoint(queue,c =>
                        {
                            foreach (var consumerType in consumerTypes)
                            {
                                c.ConfigureConsumer(cnx, consumerType);
                            }
                            
                           c.Bind(topic);
                        });
                    }
                }
                

            });
        });
        
        return serviceCollection;
    }
}



class MyClass : IConsumer<object>
{
    public Task Consume(ConsumeContext<object> context) => throw new NotImplementedException();
}
