using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs.Entities;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs;

public class AuditLogTestDbContext : DbContext
{
    public AuditLogTestDbContext(DbContextOptions<AuditLogTestDbContext> options) : base(options)
    {
    }

    public DbSet<FirstTestEntity> FirstTestEntities { get; set; } = default!;

    public DbSet<SecondTestEntity> SecondTestEntities { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FirstTestEntity>()
            .ToTable("first_test_entity")
            .HasKey(b => b.Id);

        modelBuilder.Entity<FirstTestEntity>()
            .Property(b => b.Sensitive)
            .AuditIgnore();

        modelBuilder.Entity<FirstTestEntity>()
            .HasMany(b => b.SecondEntities)
            .WithMany()
            .UsingEntity<Dictionary<object, object>>(
                l => l.HasOne<SecondTestEntity>().WithMany().HasForeignKey("SecondTestEntityId"),
                r => r.HasOne<FirstTestEntity>().WithMany().HasForeignKey("FirstTestEntityId"),
                j => { });

        modelBuilder.Entity<FirstTestEntity>()
            .OwnsOne(b => b.OwnedEntity, b =>
            {
                b.WithOwner().HasForeignKey("OwnerEntityId");
                b.ToJson("owned_entity");
            });

        modelBuilder.Entity<SecondTestEntity>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<SecondTestEntity>()
            .ToTable("second_test_entity")
            .AuditIgnore();

        modelBuilder.ApplyConfiguration(new PostgresAuditLogEntityTypeConfiguration());
    }
}
