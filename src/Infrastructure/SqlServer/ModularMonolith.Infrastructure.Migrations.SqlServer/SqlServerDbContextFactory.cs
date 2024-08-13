using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Infrastructure.DataAccess.SqlServer;
using ModularMonolith.Shared.DataAccess.EntityFramework;

namespace ModularMonolith.Infrastructure.Migrations.SqlServer;

public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    private readonly Assembly[] _configurationAssemblies =
    [
        Assembly.Load("ModularMonolith.CategoryManagement.Infrastructure"),
        Assembly.Load("ModularMonolith.Identity.Infrastructure")
    ];

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

        return new SqlServerDbContext(optionsBuilder.Options,
            ConfigurationAssemblyCollection.FromAssemblies(_configurationAssemblies));
    }
}
