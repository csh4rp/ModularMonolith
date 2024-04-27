using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLog.Models;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLog.EntityConfigurations;

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

        builder.OwnsOne(b => b.EntityKey, b =>
        {
            b.ToJson("entity_key");
        });

        builder.OwnsOne(b => b.EntityChanges, b =>
        {
            b.ToJson("entity_changes");
        });

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJson("meta_data");
        });

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EntityTypeName, b.Timestamp });
    }
}
