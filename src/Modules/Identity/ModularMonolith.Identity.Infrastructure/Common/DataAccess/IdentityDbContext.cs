using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Identity.Infrastructure.Common.DataAccess;

public sealed class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = default!;
    
    public DbSet<UserRole> UsersRoles { get; set; } = default!;
    
    public DbSet<UserClaim> UserClaims { get; set; } = default!;
    
    public DbSet<UserLogin> UserLogins { get; set; } = default!;

    public DbSet<UserToken> UserTokens { get; set; } = default!;
    
    public DbSet<Role> Roles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.HasDefaultSchema("identity");
    }
}

