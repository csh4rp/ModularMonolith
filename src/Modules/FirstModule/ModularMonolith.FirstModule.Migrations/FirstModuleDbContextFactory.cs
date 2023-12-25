using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.FirstModule.Infrastructure.Common.DataAccess;

namespace ModularMonolith.FirstModule.Migrations;

public sealed class FirstModuleDbContextFactory : IDesignTimeDbContextFactory<FirstModuleDbContext>
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
