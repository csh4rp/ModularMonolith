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
        var provider = dataAccessSection.GetSection("Provider").Value;
        var dbName = dataAccessSection.GetSection("DatabaseName").Value ?? "modular_monolith";

        switch (provider)
        {
            case "SqlServer":
                serviceCollection.AddSqlServerDataAccess<SqlServerDbContext>();
                break;
            case "Postgres":
                serviceCollection.AddPostgresDataAccess<PostgresDbContext>();
                break;
            case "Mongo":
                serviceCollection.AddMongoDataAccess(c =>
                {
                    
                },
                a =>
                {
                    
                });
                break;
        }

        return serviceCollection;
    }
}
