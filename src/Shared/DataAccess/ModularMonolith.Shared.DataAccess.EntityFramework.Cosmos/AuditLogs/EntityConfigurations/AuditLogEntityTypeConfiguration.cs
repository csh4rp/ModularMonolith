using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.AuditLogs.Models;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.AuditLogs.EntityConfigurations;

public sealed class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    private readonly string _containerName;

    public AuditLogEntityTypeConfiguration(string containerName = "AuditLogs") => _containerName = containerName;

    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToContainer(_containerName);

        builder.HasKey(b => b.Id);

        builder.HasPartitionKey(b => b.PartitionKey);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.EntityTypeName)
            .HasMaxLength(512)
            .IsRequired();

        builder.OwnsOne(b => b.EntityKey, b =>
        {
            b.ToJsonProperty(nameof(AuditLogEntity.EntityKey));
        });

        builder.OwnsOne(b => b.EntityChanges, b =>
        {
            b.ToJsonProperty(nameof(AuditLogEntity.EntityChanges));
        });

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJsonProperty(nameof(AuditLogEntity.MetaData));
        });

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EntityTypeName, b.Timestamp });
    }
}
