using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;

namespace ModularMonolith.CategoryManagement.Migrations;

public sealed class CategoryManagementDbContextFactory : IDesignTimeDbContextFactory<CategoryManagementDbContext>
{
    public CategoryManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CategoryManagementDbContext>();

        optionsBuilder.UseNpgsql(args[0], b =>
            {
                b.MigrationsAssembly(GetType().Assembly.FullName);
                b.MigrationsHistoryTable("migration_history", "category_management");
            })
            .UseSnakeCaseNamingConvention();

        var options = optionsBuilder.Options;

        return new CategoryManagementDbContext(options);
    }
}
