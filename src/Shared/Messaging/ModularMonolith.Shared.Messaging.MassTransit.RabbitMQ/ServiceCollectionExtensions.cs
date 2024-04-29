using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging.MassTransit.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQ<TDbContext>(this IServiceCollection serviceCollection,
        Assembly[] assemblies)
        where TDbContext : DbContext
    {
        serviceCollection.AddMassTransit(c =>
        {
            c.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.UsePostgres();

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


            c.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
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

                foreach (var messageType in consumerMessages.Keys)
                {
                    cfg.Publish(messageType, msg =>
                    {

                    });
                }

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
                            cf.UseEntityFrameworkOutbox<TDbContext>(context);
                        });
                    }
                }

                cfg.ConfigureEndpoints(context);
            });
        });

        return serviceCollection;
    }
}
