using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.Models;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs.Entities;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs;

public class AuditLogDbContext : DbContext
{
    public AuditLogDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<AuditLogEntity> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var firstEntityBuilder = modelBuilder.Entity<FirstTestEntity>();
        firstEntityBuilder.ToTable("FirstTestEntity");
        firstEntityBuilder.HasKey(x => x.Id);
        firstEntityBuilder.OwnsOne(b => b.OwnedEntity);
        
        firstEntityBuilder.HasMany(b => b.SecondTestEntities)
            .WithMany(b => b.FirstTestEntities)
            .UsingEntity<Dictionary<string, object>>("FirstSecondTestEntity",
                    l => l.HasOne<SecondTestEntity>().WithMany().HasForeignKey("SecondTestEntityId"),
                r => r.HasOne<FirstTestEntity>().WithMany().HasForeignKey("FirstTestEntityId"));
        
        firstEntityBuilder.Property(x => x.Timestamp).AuditIgnore();
        
        var secondEntityBuilder = modelBuilder.Entity<SecondTestEntity>();
        secondEntityBuilder.ToTable("SecondTestEntity");
        secondEntityBuilder.HasKey(x => x.Id);;
        secondEntityBuilder.AuditIgnore();

        modelBuilder.ApplyConfiguration(new AuditLogEntityTypeConfiguration());
    }
}
