using System.Reflection;
using MassTransit;
using MassTransit.RabbitMqTransport.Topology;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Domain.Abstractions;
using RabbitMQ.Client;

namespace ModularMonolith.Shared.Infrastructure.Messaging;

public static class Extensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection serviceCollection,
        Assembly[] consumerAssemblies,
        Assembly[] messageAssemblies)
    {
        serviceCollection.AddMassTransit(c =>
        {
            var consumerTypes = consumerAssemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsAssignableTo(typeof(IConsumer)))
                .ToList();

            foreach (var consumerType in consumerTypes)
            {
                c.AddConsumer(consumerType);
            }
            
            c.UsingRabbitMq((cnx, cfg) =>
            {
                var messageTypes = messageAssemblies.SelectMany(a => a.GetTypes())
                    .Where(t => t.IsAssignableTo(typeof(IEvent)))
                    .ToList();

                foreach (var messageType in messageTypes)
                {
                    cfg.Publish(messageType, msg =>
                    {
                        msg.ExchangeType = ExchangeType.Fanout;
                        msg.Durable = true;
                    });
                }
                
                
                cfg.ReceiveEndpoint("", end =>
                {
                    end.Consumer<MyClass>();
                    
                    end.Bind("", a =>
                    {
                        
                    });
                });
            });
        });
        
        return serviceCollection;
    }
}



class MyClass : IConsumer<object>
{
    public Task Consume(ConsumeContext<object> context) => throw new NotImplementedException();
}
