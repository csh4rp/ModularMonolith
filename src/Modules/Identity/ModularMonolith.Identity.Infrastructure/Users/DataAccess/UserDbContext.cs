using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Users.Entities;
using ModularMonolith.Identity.Infrastructure.Users.DataAccess.EntityConfigurations;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Identity.Infrastructure.Users.DataAccess;

internal sealed class UserDbContext : BaseDbContext
{
    public UserDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());

        modelBuilder.HasDefaultSchema("identity");
    }
}
