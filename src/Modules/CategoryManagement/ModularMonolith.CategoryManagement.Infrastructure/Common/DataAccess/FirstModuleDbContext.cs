using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;

public sealed class CategoryManagementDbContext : BaseDbContext, ICategoryDatabase
{
    public CategoryManagementDbContext(DbContextOptions<CategoryManagementDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CategoryManagementDbContext).Assembly);
        modelBuilder.HasDefaultSchema("first_module");
    }
}
