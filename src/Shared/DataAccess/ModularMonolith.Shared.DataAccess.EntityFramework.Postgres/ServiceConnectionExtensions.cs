using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Interceptors;
using ModularMonolith.Shared.DataAccess.EntityFramework.EventLogs.Interceptors;
using ModularMonolith.Shared.DataAccess.EntityFramework.Options;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.Stores;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Factories;
using ModularMonolith.Shared.DataAccess.EventLogs;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres;

public static class ServiceConnectionExtensions
{
    public static IServiceCollection AddPostgresDataAccess<TDbContext>(this IServiceCollection serviceCollection,
        Action<EntityFrameworkDataAccessOptions> optionsSetup)
        where TDbContext : DbContext
    {
        serviceCollection.AddOptionsWithValidateOnStart<EntityFrameworkDataAccessOptions>()
            .PostConfigure(optionsSetup);

        serviceCollection
            .AddScoped<PostgresConnectionFactory>()
            .AddEntityFrameworkDataAccess()
            .AddSingleton<EventLogFactory>()
            .AddScoped<IEventLogStore, EventLogStore>()
            .AddDbContextFactory<TDbContext>((serviceProvider, optionsBuilder) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var options = serviceProvider.GetRequiredService<IOptions<EntityFrameworkDataAccessOptions>>().Value;
                var connectionString = configuration.GetConnectionString(options.ConnectionStringName);

                var interceptors = new List<IInterceptor>();
                if (options.UseAuditLogInterceptor)
                {
                    interceptors.Add(new AuditLogInterceptor());
                }

                if (options.UseEventLogInterceptor)
                {
                    interceptors.Add(new EventLogInterceptor());
                }

                if (options.UseSnakeCaseNamingConvention)
                {
                    optionsBuilder.UseSnakeCaseNamingConvention();
                }

                optionsBuilder.UseNpgsql(connectionString);
                optionsBuilder.AddInterceptors(interceptors);
                optionsBuilder.UseApplicationServiceProvider(serviceProvider);
            }, ServiceLifetime.Scoped)
            .AddDbContextFactory<DbContext>((serviceProvider, _) => serviceProvider.GetRequiredService<TDbContext>(), ServiceLifetime.Scoped);

        return serviceCollection;
    }
}
