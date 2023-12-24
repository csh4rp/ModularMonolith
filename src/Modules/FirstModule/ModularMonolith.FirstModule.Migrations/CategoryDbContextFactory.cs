using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.FirstModule.Infrastructure.Categories.DataAccess;

namespace ModularMonolith.FirstModule.Migrations;

internal sealed class CategoryDbContextFactory : IDesignTimeDbContextFactory<CategoryDbContext>
{
    public CategoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CategoryDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
            })
            .UseSnakeCaseNamingConvention();

        var options = optionsBuilder.Options;

        return new CategoryDbContext(options);
    }
}
