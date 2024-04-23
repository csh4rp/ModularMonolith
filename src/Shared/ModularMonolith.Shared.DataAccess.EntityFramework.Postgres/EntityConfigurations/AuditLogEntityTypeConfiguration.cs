using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Entities;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EntityConfigurations;

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
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(b => b.EntityKey)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(b => b.EntityChanges)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJson("meta_data");
        });

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EntityTypeName, b.Timestamp });
    }
}
