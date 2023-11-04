using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLog>
{
    private readonly string? _schema;
    private readonly string _table;
    
    public AuditLogEntityTypeConfiguration(string? schema = null, string table = nameof(AuditLog))
    {
        _schema = schema;
        _table = table;
    }
    
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(_table, _schema);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));
        
        builder.Property(b => b.ChangeType)
            .HasConversion(b => b.ToString(), b => Enum.Parse<ChangeType>(b))
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(b => b.Changes)
            .IsRequired();

        builder.Property(b => b.EntityKeys)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.TraceId)
            .IsRequired()
            .HasMaxLength(32);
    }
}
