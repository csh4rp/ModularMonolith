using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.Abstract;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess;

internal sealed class FirstModuleDbContext : BaseDbContext, ICategoryDatabase
{
    public FirstModuleDbContext(DbContextOptions<FirstModuleDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FirstModuleDbContext).Assembly);
        modelBuilder.HasDefaultSchema("first_module");
    }
}
