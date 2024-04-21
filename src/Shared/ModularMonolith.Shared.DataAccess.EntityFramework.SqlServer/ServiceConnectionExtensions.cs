﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.AuditTrail.EntityFramework.Interceptors;
using ModularMonolith.Shared.Messaging.Interceptors;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer;

public static class ServiceConnectionExtensions
{
    public static IServiceCollection AddSqlServerDataAccess<TDbContext>(this IServiceCollection serviceCollection)
        where TDbContext : DbContext
    {
        serviceCollection
            .AddEntityFrameworkDataAccess()
            .AddDbContextFactory<TDbContext>((sp, optionsBuilder) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("Database");

                optionsBuilder.UseSqlServer(connectionString);
                optionsBuilder.AddInterceptors(new AuditLogInterceptor(), new PublishEventsInterceptor());
                optionsBuilder.UseApplicationServiceProvider(sp);
            }, ServiceLifetime.Scoped);

        return serviceCollection;
    }
}
