using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Events;
using Npgsql;

namespace ModularMonolith.Shared.Messaging.MassTransit.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresMessaging<TDbContext>(this IServiceCollection serviceCollection,
        string connectionString,
        Assembly[] assemblies) where TDbContext : DbContext
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        serviceCollection.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.Host = connectionStringBuilder.Host;
            options.Port = connectionStringBuilder.Port;
            options.Database = connectionStringBuilder.Database;
            options.Schema = "shared";
            options.Role = "shared";
            options.Username = connectionStringBuilder.Username;
            options.Password = connectionStringBuilder.Password;
        });

        serviceCollection.AddPostgresMigrationHostedService(true, false);

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

            c.UsingPostgres((context, cfg) =>
            {
                cfg.AutoStart = true;
                cfg.UseDbMessageScheduler();

                cfg.MessageTopology.SetEntityNameFormatter(new EventAttributeEntityNameFormatter());

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

                            cf.Subscribe(topic);
                            cf.UseEntityFrameworkOutbox<TDbContext>(context);
                        });
                    }
                }
            });

        });

        return serviceCollection;
    }
}
