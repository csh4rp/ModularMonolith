﻿using System.Reflection;
using MassTransit;
using MassTransit.SqlTransport.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging.MassTransit.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] assemblies)
        where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("Database");
        var setupConsumers = configuration.GetSection("Messaging:CustomersEnabled")
            .Get<bool>();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Database connection string is required");
        }

        serviceCollection.AddPostgresMigrationHostedService(create: true, delete: false);

        serviceCollection.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.ConnectionString = connectionString;
        });

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

            if (setupConsumers)
            {
                c.AddConsumers(assemblies);
            }

            c.UsingPostgres(connectionString, (context, configurator) =>
            {
                configurator.Host(new PostgresSqlHostSettings(connectionString));

                if (setupConsumers)
                {
                    SetupConsumers(context, configurator, assemblies);
                }
            });
        });

        return serviceCollection;
    }

    private static void SetupConsumers(IBusRegistrationContext context,
        ISqlBusFactoryConfigurator cfg,
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

                    cf.Subscribe(topic);
                });
            }
        }
    }
}
