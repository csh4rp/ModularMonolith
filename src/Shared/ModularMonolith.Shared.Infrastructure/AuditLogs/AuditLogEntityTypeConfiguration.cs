using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public sealed class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLog>
{
    private readonly bool _excludeFromMigrations;
    private readonly string _table;
    private readonly string? _schema;

    public AuditLogEntityTypeConfiguration(bool excludeFromMigrations, string table = nameof(AuditLog), string? schema = null)
    {
        _excludeFromMigrations = excludeFromMigrations;
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        if (_excludeFromMigrations)
        {
            builder.ToTable(_table, _schema,t => t.ExcludeFromMigrations());
        }
        else
        {
            builder.ToTable(_table, _schema);
        }

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));
        
        builder.Property(b => b.ChangeType)
            .HasConversion(b => b.ToString(), b => Enum.Parse<ChangeType>(b))
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(b => b.Changes)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(b => b.EntityKeys)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(b => b.ActivityId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(32);
    }
}
