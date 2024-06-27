using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.Models;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.EntityConfigurations;

public sealed class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    private readonly string _schemaName;
    private readonly string _tableName;

    public AuditLogEntityTypeConfiguration(string schemaName = "shared", string tableName = "audit_log")
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
            b.ToJson("entity_key");
            b.OwnedEntityType.AuditIgnore();
        });

        builder.OwnsMany(b => b.EntityChanges, b =>
        {
            b.ToJson("entity_changes");
            b.OwnedEntityType.AuditIgnore();
        });

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJson("meta_data");
            b.OwnedEntityType.AuditIgnore();
            // b.OwnsMany(p => p.ExtraData);
        });

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EntityTypeName, b.Timestamp });

        builder.AuditIgnore();
    }
}
