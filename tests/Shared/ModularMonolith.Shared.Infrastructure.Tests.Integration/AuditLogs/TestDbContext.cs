using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.AuditLogs;
using ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs.Entities;

namespace ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<FirstTestEntity> FirstTestEntities { get; set; } = default!;

    public DbSet<SecondTestEntity> SecondTestEntities { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FirstTestEntity>()
            .ToTable("FirstTestEntity")
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
                j => {});

        modelBuilder.Entity<FirstTestEntity>()
            .Property(b => b.OwnedEntity).HasColumnType("jsonb");
        
        modelBuilder.Entity<SecondTestEntity>()
            .ToTable("SecondTestEntity")
            .HasKey(b => b.Id);

        modelBuilder.Entity<SecondTestEntity>()
            .AuditIgnore();

        modelBuilder.ApplyConfiguration(new AuditLogEntityTypeConfiguration());
    }
}
