using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.Domain.Entities;
using EntityState = ModularMonolith.Shared.Domain.Enums.EntityState;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs.EntityConfigurations;

public sealed class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLog>
{
    private readonly bool _excludeFromMigrations;
    private readonly string _table;
    private readonly string? _schema;

    public AuditLogEntityTypeConfiguration(bool excludeFromMigrations, string table = "audit_log",
        string? schema = null)
    {
        _excludeFromMigrations = excludeFromMigrations;
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        if (_excludeFromMigrations)
        {
            builder.ToTable(_table, _schema, t => t.ExcludeFromMigrations());
        }
        else
        {
            builder.ToTable(_table, _schema);
        }

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.EntityState)
            .HasConversion(b => b.ToString(), b => Enum.Parse<EntityState>(b))
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(b => b.EntityPropertyChanges)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(b => b.EntityKeys)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(b => b.OperationName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.TraceId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(32);
        
        builder.Property(b => b.SpanId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(16);
        
        builder.Property(b => b.ParentSpanId)
            .IsUnicode(false)
            .HasMaxLength(16);

        builder.Property(b => b.UserName)
            .HasMaxLength(128);
        
        builder.Property(b => b.IpAddress)
            .HasMaxLength(32);

        builder.Property(b => b.UserAgent)
            .HasMaxLength(256);
        
        // TODO: Add index for EntityKey, EntityType [{"PropertyName": "id", "Value": "GUID"}]
    }
}
