﻿using System.Reflection;
using Confluent.Kafka;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Infrastructure.Messaging.Kafka;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies) where TDbContext : DbContext
    {
        var options = configuration.GetSection("Kafka")
                          .Get<KafkaOptions>()
                      ?? throw new NullReferenceException("Kafka section is missing");

        var provider = configuration.GetSection("DataAccessProvider").Value;

        serviceCollection
            .AddScoped<IMessageBus, MessageBus>()
            .AddMassTransit(c =>
            {
                c.AddEntityFrameworkOutbox<TDbContext>(o =>
                {
                    switch (provider)
                    {
                        case "Postgres":
                            o.UsePostgres();
                            break;
                        case "SqlServer":
                            o.UseSqlServer();
                            break;
                    }

                    o.UseBusOutbox(cfg =>
                    {
                        cfg.MessageDeliveryLimit = 10;
                    });
                });

                c.AddConsumers(consumerAssemblies);

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

                        configurator.TopicEndpoint<string, object>("", "", e =>
                        {

                        });

                    });
                });
            });

        return serviceCollection;
    }
}
