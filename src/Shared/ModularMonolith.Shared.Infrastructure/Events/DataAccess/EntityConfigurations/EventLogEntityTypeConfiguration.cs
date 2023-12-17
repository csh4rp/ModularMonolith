using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

internal sealed class EventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLog>
{
    private readonly bool _excludeFromMigrations;
    private readonly string _table;
    private readonly string? _schema;

    public EventLogEntityTypeConfiguration(bool excludeFromMigrations, string table = nameof(EventLog), string? schema = null)
    {
        _excludeFromMigrations = excludeFromMigrations;
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<EventLog> builder)
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

        builder.Property(b => b.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(b => b.Type)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(b => b.ActivityId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(32);

        builder.Property(b => b.OperationName)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(b => b.PublishedAt);
    }
}
