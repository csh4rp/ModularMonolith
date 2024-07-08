using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ModularMonolith.Infrastructure.DataAccess.SqlServer;

public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

        optionsBuilder.UseSqlServer(args[0], b =>
        {
            b.MigrationsAssembly(GetType().Assembly.FullName);
            b.MigrationsHistoryTable("MigrationHistory", "Shared");
        });

        var options = optionsBuilder.Options;

        return new SqlServerDbContext(options, []);
    }
}
