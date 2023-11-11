using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.Infrastructure.Events;

internal sealed class EventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLog>
{
    private readonly string _table;
    private readonly string? _schema;

    public EventLogEntityTypeConfiguration(string table = nameof(EventLog), string? schema = null)
    {
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable(_table, _schema);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.Payload)
            .IsRequired();

        builder.Property(b => b.Type)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.Topic)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.Stream)
            .HasMaxLength(128);
        
        builder.Property(b => b.TraceId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(32);
    }
}
