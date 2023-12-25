using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Application.Categories.Abstract;
using ModularMonolith.FirstModule.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.FirstModule.Infrastructure.Common.DataAccess;

public sealed class FirstModuleDbContext : BaseDbContext, ICategoryDatabase
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
