using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.AuditTrail.Storage.Interceptors;
using ModularMonolith.Shared.DataAccess.Factories;
using ModularMonolith.Shared.DataAccess.SqlServer.Factories;
using ModularMonolith.Shared.Messaging.Interceptors;

namespace ModularMonolith.Shared.DataAccess.SqlServer;

public static class ServiceConnectionExtensions
{
    public static IServiceCollection AddSqlServerDataAccess<TDbContext>(this IServiceCollection serviceCollection, string connectionString)
        where TDbContext : DbContext
    {
        serviceCollection.AddSingleton<IDbConnectionFactory, SqlServerDbConnectionFactory>()
            .AddDbContextFactory<TDbContext>((sp, optionsBuilder) =>
            {
                optionsBuilder.UseSqlServer(connectionString);
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
