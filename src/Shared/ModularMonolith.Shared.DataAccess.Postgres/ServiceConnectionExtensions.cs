using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.AuditTrail.Storage.Interceptors;
using ModularMonolith.Shared.DataAccess.Factories;
using ModularMonolith.Shared.DataAccess.Postgres.Factories;
using ModularMonolith.Shared.Messaging.Interceptors;

namespace ModularMonolith.Shared.DataAccess.Postgres;

public static class ServiceConnectionExtensions
{
    public static IServiceCollection AddPostgresDataAccess<TDbContext>(this IServiceCollection serviceCollection, string connectionString)
        where TDbContext : DbContext
    {
        serviceCollection.AddSingleton<IDbConnectionFactory, PostgresDbConnectionFactory>()
            .AddDbContextFactory<TDbContext>((sp, optionsBuilder) =>
            {
                optionsBuilder.UseNpgsql(connectionString);
                optionsBuilder.UseSnakeCaseNamingConvention();
                optionsBuilder.AddInterceptors(new AuditLogInterceptor(), new PublishEventsInterceptor());
                optionsBuilder.UseApplicationServiceProvider(sp);
            }, ServiceLifetime.Scoped)
            .AddDataAccess(c =>
            {
                c.ConnectionString = connectionString;
            });

        return serviceCollection;
    }
}
