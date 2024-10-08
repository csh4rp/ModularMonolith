﻿using System.Reflection;
using Confluent.Kafka;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Messaging.MassTransit;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;
using Quartz;

namespace ModularMonolith.Infrastructure.Messaging.Kafka;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies,
        bool runConsumers) where TDbContext : DbContext
    {
        var options = configuration.GetSection("Kafka")
                          .Get<KafkaOptions>()
                      ?? throw new NullReferenceException("Kafka section is missing");

        var provider = configuration.GetSection("DataAccess:Provider").Value;

        serviceCollection.AddQuartz()
            .AddQuartzHostedService();

        serviceCollection
            .AddScoped<IMessageBus, MessageBus>()
            .AddScoped<EventLogEntryFactory>()
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

                    o.UseBusOutbox();
                });

                c.AddQuartzConsumers();
                c.AddMessageScheduler(MessagingConstants.ScheduleQueueUri);
                c.SetJobConsumerOptions();
                c.AddJobSagaStateMachines().EntityFrameworkRepository(cf =>
                {
                    cf.ExistingDbContext<TDbContext>();
                    switch (provider)
                    {
                        case "Postgres":
                            cf.UsePostgres();
                            break;
                        case "SqlServer":
                            cf.UseSqlServer();
                            break;
                    }
                });

                if (runConsumers)
                {
                    c.AddConsumers(consumerAssemblies);
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
                    });
                });
            });

        serviceCollection.AddHealthChecks()
            .AddKafka(
                new ProducerConfig
                {
                    SaslUsername = options.Username,
                    SaslPassword = options.Password,
                    BootstrapServers = options.Host
                }, tags: ["live", "ready"]);

        return serviceCollection;
    }
}
