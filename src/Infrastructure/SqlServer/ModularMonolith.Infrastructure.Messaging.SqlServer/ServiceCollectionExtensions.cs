using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Messaging.MassTransit;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;
using ModularMonolith.Shared.Messaging.MassTransit.Filters;
using Quartz;

namespace ModularMonolith.Infrastructure.Messaging.SqlServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies,
        bool runConsumers) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("Messaging")
                               ?? configuration.GetConnectionString("Database")
                               ?? throw new ArgumentNullException("ConnectionString:Messaging");

        serviceCollection.AddQuartz()
            .AddQuartzHostedService();

        serviceCollection
            .AddScoped<IMessageBus, MessageBus>()
            .AddScoped<EventLogEntryFactory>()
            .AddMassTransit(c =>
            {
                c.AddEntityFrameworkOutbox<TDbContext>(o =>
                {
                    o.UseSqlServer();
                });

                c.AddQuartzConsumers();
                c.AddMessageScheduler(MessagingConstants.ScheduleQueueUri);
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

                c.UsingSqlServer((context, configurator) =>
                {
                    configurator.UseMessageScheduler(MessagingConstants.ScheduleQueueUri);
                    configurator.UseConsumeFilter(typeof(IdentityFilter<>), context);
                    configurator.ConfigureEndpoints(context);
                });
            });

        serviceCollection.AddHealthChecks()
            .AddSqlServer(connectionString, tags: ["live", "ready"]);

        serviceCollection.AddOptions<SqlTransportOptions>()
            .Configure(options =>
            {
                options.ConnectionString = connectionString;
                options.Schema = "transport";
            });

        return serviceCollection;
    }
}
