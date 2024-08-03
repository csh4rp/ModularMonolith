using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastructure.DataAccess.Postgres;
using ModularMonolith.Shared.DataAccess.EntityFramework;

namespace ModularMonolith.Infrastructure.Migrations.Postgres;

public class PostgresDbContextFactory : IDesignTimeDbContextFactory<PostgresDbContext>
{
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

        var options = optionsBuilder.Options;

        if (args.Length > 1)
        {
            var assemblyNames = args[1..];

            var assemblies = new List<Assembly>(assemblyNames.Length);
            foreach (var assemblyName in assemblyNames)
            {
                assemblies.Add(Assembly.Load(assemblyName));
            }

            return new PostgresDbContext(options, ConfigurationAssemblyCollection.FromAssemblies(assemblies));
        }

        return new PostgresDbContext(options, ConfigurationAssemblyCollection.Empty);
    }
}
