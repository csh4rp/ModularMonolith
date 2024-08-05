using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Infrastructure.DataAccess.SqlServer;
using ModularMonolith.Shared.DataAccess.EntityFramework;

namespace ModularMonolith.Infrastructure.Migrations.SqlServer;

public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException(
                "Connection string is required. Usage: dotnet ef migrations add <MigrationName> -- <ConnectionString>");
        }

        var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();

        optionsBuilder.UseSqlServer(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
                b.MigrationsHistoryTable("migration_history", "shared");
            });

        var options = optionsBuilder.Options;

        if (args.Length > 1)
        {
            var assemblyNames = args[1..];

            var assemblies = new List<Assembly>(assemblyNames.Length);
            foreach (var assemblyName in assemblyNames)
            {
                assemblies.Add(Assembly.Load(assemblyName));
            }

            return new SqlServerDbContext(options, ConfigurationAssemblyCollection.FromAssemblies(assemblies));
        }

        return new SqlServerDbContext(options, ConfigurationAssemblyCollection.Empty);
    }
}
