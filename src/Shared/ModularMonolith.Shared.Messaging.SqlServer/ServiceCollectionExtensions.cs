using System.Reflection;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging.SqlServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerMessaging<TDbContext>(this IServiceCollection serviceCollection,
        string connectionString,
        Assembly[] assemblies) where TDbContext : DbContext
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        serviceCollection.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            var dataSource = connectionStringBuilder.DataSource;
            var dataSourceParts = dataSource.Split(',');
            var host = dataSourceParts.Length == 1 ? dataSource : dataSourceParts[0];
            var port = dataSourceParts.Length == 1 ? (int?)null : int.Parse(dataSourceParts[1]);

            options.Host = connectionStringBuilder.DataSource;
            options.Port = port;
            options.Database = connectionStringBuilder.InitialCatalog;
            options.Schema = "Shared";
            options.Role = "Shared";
            options.Username = connectionStringBuilder.UserID;
            options.Password = connectionStringBuilder.Password;
        });

        serviceCollection.AddSqlServerMigrationHostedService(true, false);

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

            c.UsingSqlServer((context, cfg) =>
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
                        });
                    }
                }
            });

        });

        return serviceCollection;
    }
}
