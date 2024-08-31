using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.DataAccess.Postgres;
using ModularMonolith.Infrastructure.DataAccess.SqlServer;
using ModularMonolith.Shared.DataAccess.EntityFramework;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer;

namespace ModularMonolith.Infrastructure.DataAccess;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        IReadOnlyCollection<Assembly> assemblies)
    {
        var collection = ConfigurationAssemblyCollection.FromAssemblies(assemblies);
        serviceCollection.Add(new ServiceDescriptor(typeof(ConfigurationAssemblyCollection), _ => collection,
            ServiceLifetime.Singleton));

        var dataAccessSection = configuration.GetSection("DataAccess");
        var provider = dataAccessSection.GetSection("Provider").Value;

        if (string.IsNullOrEmpty(provider))
        {
            throw new ArgumentException("DataAccess:Provider is required");
        }

        switch (provider)
        {
            case "SqlServer":
                serviceCollection.AddSqlServerDataAccess<SqlServerDbContext>(options =>
                {
                    options.UseAuditLogInterceptor = true;
                    options.UseEventLogInterceptor = true;
                    options.ConnectionStringName = "Database";
                })
                .AddHealthChecks()
                .AddDbContextCheck<SqlServerDbContext>(tags: ["live", "ready"]);
                break;
            case "Postgres":
                serviceCollection.AddPostgresDataAccess<PostgresDbContext>(options =>
                {
                    options.UseAuditLogInterceptor = true;
                    options.UseEventLogInterceptor = true;
                    options.UseSnakeCaseNamingConvention = true;
                    options.ConnectionStringName = "Database";
                })
                .AddHealthChecks()
                .AddDbContextCheck<PostgresDbContext>(tags: ["live", "ready"]);
                break;
        }

        return serviceCollection;
    }
}
