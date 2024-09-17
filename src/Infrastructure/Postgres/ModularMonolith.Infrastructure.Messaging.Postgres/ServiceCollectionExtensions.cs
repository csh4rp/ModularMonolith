using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;
using ModularMonolith.Shared.Messaging.MassTransit.Filters;
using Quartz;

namespace ModularMonolith.Infrastructure.Messaging.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies,
        bool runConsumers) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("Messaging")!;

        serviceCollection.AddQuartz()
            .AddQuartzHostedService();

        serviceCollection
            .AddScoped<IMessageBus, MessageBus>()
            .AddScoped<EventLogEntryFactory>()
            .AddMassTransit(c =>
            {
                c.AddEntityFrameworkOutbox<TDbContext>(o =>
                {
                    o.UsePostgres();
                });

                c.AddQuartzConsumers();
                c.AddMessageScheduler(new Uri("queue:quartz"));

                c.SetJobConsumerOptions();
                c.AddJobSagaStateMachines().EntityFrameworkRepository(cf =>
                {
                    cf.ExistingDbContext<TDbContext>();
                    cf.UsePostgres();
                });

                if (runConsumers)
                {
                    c.AddConsumers(consumerAssemblies);
                }

                c.UsingPostgres((context, configurator) =>
                {
                    configurator.UseMessageScheduler(new Uri("queue:quartz"));
                    configurator.UseConsumeFilter(typeof(IdentityFilter<>), context);
                    configurator.ConfigureEndpoints(context);
                });
            });

        serviceCollection.AddHealthChecks()
            .AddNpgSql(connectionString, tags: ["live", "ready"]);

        serviceCollection.AddOptions<SqlTransportOptions>()
            .Configure(options =>
            {
                options.ConnectionString = connectionString;
                options.Schema = "transport";
            });

        return serviceCollection;
    }
}
