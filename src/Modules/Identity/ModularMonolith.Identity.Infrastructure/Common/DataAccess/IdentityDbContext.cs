using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Shared.Infrastructure.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

namespace ModularMonolith.Identity.Infrastructure.Common.DataAccess;

public sealed class IdentityDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public IdentityDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public sealed override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            _ = await Database.BeginTransactionAsync(cancellationToken);
            TransactionalScope.Current.Value!.EnlistTransaction(this);
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            _ = Database.BeginTransaction();
            TransactionalScope.Current.Value!.EnlistTransaction(this);
        }

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.HasDefaultSchema("identity");
        
        modelBuilder.ApplyConfiguration(new EventCorrelationLockEntityTypeConfiguration(true, schema: "shared"))
            .ApplyConfiguration(new EventLogLockEntityTypeConfiguration(true, schema: "shared"))
            .ApplyConfiguration(new EventLogEntityTypeConfiguration(true, schema: "shared"))
            .ApplyConfiguration(new EventLogPublishAttemptEntityTypeConfiguration(true, schema: "shared"))
            .ApplyConfiguration(new AuditLogEntityTypeConfiguration(true, schema: "shared"));
    }
}

