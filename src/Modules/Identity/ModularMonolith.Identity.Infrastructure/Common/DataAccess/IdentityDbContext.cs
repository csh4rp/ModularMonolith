using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Users.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Identity.Infrastructure.Common.DataAccess;

internal sealed class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.HasDefaultSchema("identity");
    }
}
