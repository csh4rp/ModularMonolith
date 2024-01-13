using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.Abstract;

public interface IIdentityDatabase
{
    DbSet<User> Users { get; }
    
    DbSet<UserRole> UserRoles { get; }
    
    DbSet<UserClaim> UserClaims { get; }
    
    DbSet<UserLogin> UserLogins { get; }
    
    DbSet<UserToken> UserTokens { get; }
    
    DbSet<Role> Roles { get; }
    
    DbSet<RoleClaim> RoleClaims { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
