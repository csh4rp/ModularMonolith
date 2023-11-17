using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public class AuditLogEntityTypeConfiguration(string table = nameof(AuditLog), string? schema = null)
    : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(table, schema);

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

        builder.Property(b => b.TraceId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(32);
    }
}
