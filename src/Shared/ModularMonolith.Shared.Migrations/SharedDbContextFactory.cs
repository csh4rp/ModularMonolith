﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Shared.Migrations;

public sealed class SharedDbContextFactory : IDesignTimeDbContextFactory<SharedDbContext>
{
    public SharedDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SharedDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
                b.MigrationsHistoryTable("migration_history", "shared");
            })
            .UseSnakeCaseNamingConvention();

        var options = optionsBuilder.Options;

        return new SharedDbContext(options);
    }
}
