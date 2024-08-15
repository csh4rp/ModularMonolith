using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.Models;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs.Entities;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs;

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
        firstEntityBuilder.Property(x => x.Timestamp).AuditIgnore();
        firstEntityBuilder.OwnsOne(b => b.FirstOwnedEntity, b =>
        {
            b.ToJson();
            b.OwnedEntityType.AuditIgnore();
        });
        firstEntityBuilder.OwnsOne(b => b.SecondOwnedEntity, b =>
        {
            b.ToJson();
            b.WithOwner();
        });

        firstEntityBuilder.HasMany(b => b.SecondTestEntities)
            .WithMany(b => b.FirstTestEntities)
            .UsingEntity<Dictionary<string, object>>("FirstSecondTestEntity",
                l => l.HasOne<SecondTestEntity>().WithMany().HasForeignKey("SecondTestEntityId"),
                r => r.HasOne<FirstTestEntity>().WithMany().HasForeignKey("FirstTestEntityId"));

        var secondEntityBuilder = modelBuilder.Entity<SecondTestEntity>();
        secondEntityBuilder.ToTable("SecondTestEntity");
        secondEntityBuilder.HasKey(x => x.Id);
        ;
        secondEntityBuilder.AuditIgnore();

        modelBuilder.ApplyConfiguration(new AuditLogEntityTypeConfiguration());
    }
}
