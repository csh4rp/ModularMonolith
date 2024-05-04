using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Interceptors;
using ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.Options;
using ModularMonolith.Shared.DataAccess.EntityFramework.EventLogs.Interceptors;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos;

public static class ServiceConnectionExtensions
{
    public static IServiceCollection AddCosmosDataAccess<TDbContext>(this IServiceCollection serviceCollection,
        Action<CosmosOptions> optionsSetup)
        where TDbContext : DbContext
    {
        serviceCollection.AddOptionsWithValidateOnStart<CosmosOptions>()
            .PostConfigure(optionsSetup);

        serviceCollection
            .AddScoped<CosmosClientFactory>()
            .AddEntityFrameworkDataAccess()
            .AddDbContextFactory<TDbContext>((serviceProvider, optionsBuilder) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<CosmosOptions>>().Value;

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

                if (options.TokenCredential is not null)
                {
                    optionsBuilder.UseCosmos(options.AccountEndpoint,
                        options.TokenCredential,
                        options.DatabaseName,
                        c =>
                        {

                        });
                }
                else if (options.AccountKey is not null)
                {
                    optionsBuilder.UseCosmos(options.AccountEndpoint,
                        options.AccountKey,
                        options.DatabaseName, c =>
                        {

                        });
                }
                else
                {
                    ArgumentNullException.ThrowIfNull(options.AccountKey);
                }

                optionsBuilder.AddInterceptors(interceptors);
                optionsBuilder.UseApplicationServiceProvider(serviceProvider);
            }, ServiceLifetime.Scoped);

        return serviceCollection;
    }
}
