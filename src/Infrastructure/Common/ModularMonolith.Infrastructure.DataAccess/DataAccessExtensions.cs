﻿using Microsoft.Extensions.Configuration;
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

        switch (provider)
        {
            case "SqlServer":
                serviceCollection.AddSqlServerDataAccess<SqlServerDbContext>(options =>
                {
                    options.UseAuditLogInterceptor = true;
                    options.UseEventLogInterceptor = true;
                    options.ConnectionStringName = "Database";
                });
                break;
            case "Postgres":
                serviceCollection.AddPostgresDataAccess<PostgresDbContext>(options =>
                {
                    options.UseAuditLogInterceptor = true;
                    options.UseEventLogInterceptor = true;
                    options.ConnectionStringName = "Database";
                });
                break;
            case "Mongo":
                serviceCollection.AddMongoDataAccess(options =>
                {
                    options.ConnectionStringName = "Database";
                    options.AuditLogOptions.CollectionName = "audit_logs";
                    options.AuditLogOptions.ChangeDataCaptureDiff
                });
                break;
        }

        return serviceCollection;
    }
}
