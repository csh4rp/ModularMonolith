using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Identity.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;

namespace ModularMonolith.Identity.Migrations;

public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
                b.MigrationsHistoryTable("migration_history", "identity");
            })
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new AuditLogInterceptor());

        var options = optionsBuilder.Options;

        return new IdentityDbContext(options);
    }
}
