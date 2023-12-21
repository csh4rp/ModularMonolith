using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess;

namespace ModularMonolith.Modules.FirstModule.Migrations;

internal sealed class DbContextFactory : IDesignTimeDbContextFactory<FirstModuleDbContext>
{
    public FirstModuleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FirstModuleDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
            })
            .UseSnakeCaseNamingConvention();

        var options = optionsBuilder.Options;

        return new FirstModuleDbContext(options);
    }
}
