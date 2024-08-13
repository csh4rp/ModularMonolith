using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastructure.DataAccess.Postgres;
using ModularMonolith.Shared.DataAccess.EntityFramework;

namespace ModularMonolith.Infrastructure.Migrations.Postgres;

public class PostgresDbContextFactory : IDesignTimeDbContextFactory<PostgresDbContext>
{
    private readonly Assembly[] _configurationAssemblies =
    [
        Assembly.Load("ModularMonolith.CategoryManagement.Infrastructure"),
        Assembly.Load("ModularMonolith.Identity.Infrastructure")
    ];

    public PostgresDbContext CreateDbContext(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException(
                "Connection string is required. Usage: dotnet ef migrations add <MigrationName> -- <ConnectionString>");
        }

        var optionsBuilder = new DbContextOptionsBuilder<PostgresDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
                b.MigrationsHistoryTable("migration_history", "shared");
            })
            .UseSnakeCaseNamingConvention();

        return new PostgresDbContext(optionsBuilder.Options,
            ConfigurationAssemblyCollection.FromAssemblies(_configurationAssemblies));
    }
}
