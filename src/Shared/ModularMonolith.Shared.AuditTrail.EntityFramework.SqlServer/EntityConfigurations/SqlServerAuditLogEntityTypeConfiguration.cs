using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.AuditTrail.EntityFramework.SqlServer.EntityConfigurations;

public sealed class SqlServerAuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLog>
{
    private readonly string _schemaName;
    private readonly string _tableName;

    public SqlServerAuditLogEntityTypeConfiguration(string schemaName = "Shared", string tableName = nameof(AuditLog))
    {
        _schemaName = schemaName;
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(_tableName, _schemaName);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.EntityState)
            .HasConversion(b => b.ToString(), b => Enum.Parse<EntityState>(b))
            .IsRequired()
            .HasMaxLength(8);

        builder.OwnsMany(b => b.EntityPropertyChanges, b =>
        {
            b.ToTable(_tableName, _schemaName);
            b.ToJson(nameof(AuditLog.EntityPropertyChanges));
        });

        builder.OwnsMany(b => b.EntityKeys, b =>
        {
            b.ToTable(_tableName, _schemaName);
            b.ToJson(nameof(AuditLog.EntityKeys));
        });

        builder.Property(b => b.EntityType)
            .IsRequired()
            .HasMaxLength(256);

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToTable(_tableName, _schemaName);
            b.ToJson(nameof(AuditLog.MetaData));
        });

        // TODO: Add index for EntityKey, EntityType [{"PropertyName": "id", "Value": "GUID"}]
    }
}
