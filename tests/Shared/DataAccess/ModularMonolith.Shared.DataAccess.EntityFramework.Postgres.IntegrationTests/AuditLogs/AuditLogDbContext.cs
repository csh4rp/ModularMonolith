using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs.Entities;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs;

public class AuditLogDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var firstEntityBuilder = modelBuilder.Entity<FirstTestEntity>();
        firstEntityBuilder.ToTable("FirstTestEntity");
        firstEntityBuilder.HasKey(x => x.Id);
        firstEntityBuilder.OwnsOne(b => b.OwnedEntity);
        
        firstEntityBuilder.HasMany(b => b.SecondTestEntities)
            .WithMany(b => b.FirstTestEntities)
            .UsingEntity("FirstSecondTestEntity",
                    l => l.HasOne(typeof(FirstTestEntity)).WithMany().HasForeignKey("FirstTestEntityId").HasPrincipalKey(nameof(FirstTestEntity.Id)),
                r => r.HasOne(typeof(SecondTestEntity)).WithMany().HasForeignKey("SecondTestEntityId").HasPrincipalKey(nameof(SecondTestEntity.Id)),
                j => j.HasKey("FirstTestEntityId", "SecondTestEntityId"));
        
        firstEntityBuilder.Property(x => x.Timestamp).AuditIgnore();
        
        
        var secondEntityBuilder = modelBuilder.Entity<SecondTestEntity>();
        secondEntityBuilder.ToTable("SecondTestEntity");
        secondEntityBuilder.HasKey(x => x.Id);;
        secondEntityBuilder.AuditIgnore();
    }
}
