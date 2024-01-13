using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Infrastructure.Common.Abstract;

namespace ModularMonolith.Identity.Infrastructure.Common.Concrete;

internal sealed class IdentityDatabase : IIdentityDatabase
{
    private readonly DbContext _dbContext;

    public IdentityDatabase(DbContext dbContext) => _dbContext = dbContext;

    public DbSet<User> Users => _dbContext.Set<User>();
    
    public DbSet<UserRole> UserRoles => _dbContext.Set<UserRole>();
    
    public DbSet<UserClaim> UserClaims => _dbContext.Set<UserClaim>();
    
    public DbSet<UserLogin> UserLogins => _dbContext.Set<UserLogin>();
    
    public DbSet<UserToken> UserTokens => _dbContext.Set<UserToken>();
    
    public DbSet<Role> Roles => _dbContext.Set<Role>();

    public DbSet<RoleClaim> RoleClaims => _dbContext.Set<RoleClaim>();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
