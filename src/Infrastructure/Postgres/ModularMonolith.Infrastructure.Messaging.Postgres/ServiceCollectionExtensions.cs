using System.Reflection;
using MassTransit;
using MassTransit.SqlTransport.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Infrastructure.Messaging.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresMessaging<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] consumerAssemblies) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("Messaging")!;
        var provider = configuration.GetSection("DataAccess:Provider").Value;

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

                c.UsingPostgres((context, configurator) =>
                {
                    configurator.Host(new PostgresSqlHostSettings(connectionString));
                    configurator.ConfigureEndpoints(context);
                });
            });

        return serviceCollection;
    }
}
