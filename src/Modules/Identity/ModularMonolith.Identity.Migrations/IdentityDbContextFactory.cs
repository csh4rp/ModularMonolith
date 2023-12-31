using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Identity.Infrastructure.Common.DataAccess;

[assembly: ExcludeFromCodeCoverage]

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
            .UseSnakeCaseNamingConvention();

        var options = optionsBuilder.Options;

        return new IdentityDbContext(options);
    }
}
