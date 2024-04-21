using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.DataAccess.Postgres;
using ModularMonolith.Infrastructure.DataAccess.SqlServer;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer;
using ModularMonolith.Shared.DataAccess.Mongo;

namespace ModularMonolith.Infrastructure.DataAccess;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var dataAccessSection = configuration.GetSection("DataAccess");
        var provider = dataAccessSection.GetValue<string>("Provider");
        var dbName = dataAccessSection.GetValue<string>("DatabaseName") ?? "modular_monolith";

        switch (provider)
        {
            case "SqlServer":
                serviceCollection.AddSqlServerDataAccess<SqlServerDbContext>();
                break;
            case "Postgres":
                serviceCollection.AddPostgresDataAccess<PostgresDbContext>();
                break;
            case "Mongo":
                serviceCollection.AddMongoDataAccess(dbName);
                break;
        }

        return serviceCollection;
    }
}
