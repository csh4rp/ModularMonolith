using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.Models;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.EntityConfigurations;

public sealed class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    private readonly string _schemaName;
    private readonly string _tableName;

    public AuditLogEntityTypeConfiguration(string schemaName = "Shared", string tableName = "AuditLog")
    {
        _schemaName = schemaName;
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToTable(_tableName, _schemaName);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.EntityTypeName)
            .HasMaxLength(512)
            .IsRequired();

        builder.OwnsMany(b => b.EntityKey, b =>
        {
            b.ToJson(nameof(AuditLogEntity.EntityKey));
            b.OwnedEntityType.AuditIgnore();
        });

        builder.OwnsMany(b => b.EntityChanges, b =>
        {
            b.ToJson(nameof(AuditLogEntity.EntityChanges));
            b.OwnedEntityType.AuditIgnore();
        });

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJson(nameof(AuditLogEntity.MetaData));
            b.OwnsMany(c => c.ExtraData);
            b.OwnedEntityType.AuditIgnore();
        });

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EntityTypeName, b.Timestamp });

        builder.AuditIgnore();
    }
}
