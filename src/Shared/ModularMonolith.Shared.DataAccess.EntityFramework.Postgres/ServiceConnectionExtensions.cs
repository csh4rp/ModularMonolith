using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLog.Interceptors;
using ModularMonolith.Shared.Messaging.Interceptors;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres;

public static class ServiceConnectionExtensions
{
    public static IServiceCollection AddPostgresDataAccess<TDbContext>(this IServiceCollection serviceCollection)
        where TDbContext : DbContext
    {
        serviceCollection
            .AddEntityFrameworkDataAccess()
            .AddDbContextFactory<TDbContext>((sp, optionsBuilder) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("Database");

                optionsBuilder.UseNpgsql(connectionString);
                optionsBuilder.UseSnakeCaseNamingConvention();
                optionsBuilder.AddInterceptors(new AuditLogInterceptor(), new PublishEventsInterceptor());
                optionsBuilder.UseApplicationServiceProvider(sp);
            }, ServiceLifetime.Scoped);

        return serviceCollection;
    }
}
