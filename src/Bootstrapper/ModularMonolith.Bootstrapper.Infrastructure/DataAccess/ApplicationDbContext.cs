using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

namespace ModularMonolith.Bootstrapper.Infrastructure.DataAccess;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<EventLog> EventLogs { get; set; } = default!;
    
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            _ = await Database.BeginTransactionAsync(cancellationToken);
            TransactionalScope.Current.Value!.EnlistTransaction(this);
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
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
        modelBuilder.ApplyConfiguration(new EventLogEntityTypeConfiguration(true, schema: "shared"))
            .ApplyConfiguration(new AuditLogEntityTypeConfiguration(true, schema: "shared"));

        modelBuilder.AddInboxStateEntity(c => c.ToTable("inbox_state", "shared"));
        modelBuilder.AddOutboxStateEntity(c => c.ToTable("outbox_state", "shared"));
        modelBuilder.AddOutboxMessageEntity(c => c.ToTable("outbox_message", "shared"));

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("ModularMonolith.CategoryManagement.Infrastructure"));
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("ModularMonolith.Identity.Infrastructure"));
    }
}
