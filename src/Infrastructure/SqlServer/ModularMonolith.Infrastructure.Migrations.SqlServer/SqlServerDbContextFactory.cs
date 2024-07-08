using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Infrastructure.DataAccess.SqlServer;

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
        
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

        optionsBuilder.UseSqlServer(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
                b.MigrationsHistoryTable("migration_history", "shared");
            });

        var options = optionsBuilder.Options;

        return new SqlServerDbContext(options, []);
    }
}
