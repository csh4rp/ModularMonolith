using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastructure.DataAccess.Postgres;

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
        
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
                b.MigrationsHistoryTable("migration_history", "shared");
            })
            .UseSnakeCaseNamingConvention();

        var options = optionsBuilder.Options;

        return new PostgresDbContext(options, []);
    }
}
