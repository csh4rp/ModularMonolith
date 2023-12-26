using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;

namespace ModularMonolith.CategoryManagement.Migrations;

public sealed class CategoryManagementDbContextFactory : IDesignTimeDbContextFactory<CategoryManagementDbContext>
{
    public CategoryManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CategoryManagementDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
        {
            b.MigrationsAssembly(GetType().Assembly.FullName);
            b.MigrationsHistoryTable("migration_history", "first_module");
        })
        .UseSnakeCaseNamingConvention();

        var options = optionsBuilder.Options;

        return new CategoryManagementDbContext(options);
    }
}
