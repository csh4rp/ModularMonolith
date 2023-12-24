using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Application.Categories.Abstract;
using ModularMonolith.FirstModule.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.FirstModule.Infrastructure.Categories.DataAccess;

internal sealed class CategoryDbContext : BaseDbContext, ICategoryDatabase
{
    public CategoryDbContext(DbContextOptions<CategoryDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CategoryDbContext).Assembly);
        modelBuilder.HasDefaultSchema("first_module");
    }
}
