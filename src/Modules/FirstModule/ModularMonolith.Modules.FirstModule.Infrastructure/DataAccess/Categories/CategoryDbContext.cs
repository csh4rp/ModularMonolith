using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.Abstract;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess.Categories;

public sealed class CategoryDbContext : BaseDbContext, ICategoryDatabase
{
    public CategoryDbContext(DbContextOptions<CategoryDbContext> options) : base(options)
    {
    }
    
    public DbSet<Category> Categories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CategoryDbContext).Assembly);
    }
}
