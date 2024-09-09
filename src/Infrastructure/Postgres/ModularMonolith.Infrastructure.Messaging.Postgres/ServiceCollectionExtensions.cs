using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;
using ModularMonolith.Shared.Messaging.MassTransit.Filters;

namespace ModularMonolith.Infrastructure.Messaging.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies,
        bool runConsumers) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("Messaging")!;
        var provider = configuration.GetSection("DataAccess").GetValue<string>("Provider");

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
                });

                if (runConsumers)
                {
                    c.AddConsumers(consumerAssemblies);
                }

                c.UsingPostgres((context, configurator) =>
                {
                    configurator.UseSqlMessageScheduler();

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

        serviceCollection.AddPostgresMigrationHostedService();

        return serviceCollection;
    }
}
