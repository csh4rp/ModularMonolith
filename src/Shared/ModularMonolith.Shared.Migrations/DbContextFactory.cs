using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Shared.Infrastructure.DataAccess.Internal;

namespace ModularMonolith.Shared.Migrations;

internal sealed class DbContextFactory : IDesignTimeDbContextFactory<InternalDbContext>
{
    public InternalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InternalDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
            })
        .UseSnakeCaseNamingConvention();
        
        var options = optionsBuilder.Options;

        return new InternalDbContext(options);
    }
}
