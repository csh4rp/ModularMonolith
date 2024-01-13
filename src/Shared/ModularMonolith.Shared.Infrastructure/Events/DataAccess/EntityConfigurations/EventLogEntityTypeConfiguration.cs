using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

public sealed class EventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLog>
{
    private readonly bool _excludeFromMigrations;
    private readonly string _table;
    private readonly string? _schema;

    public EventLogEntityTypeConfiguration(bool excludeFromMigrations,
        string table = "event_log",
        string? schema = null)
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

        builder.Property(b => b.EventPayload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(b => b.EventType)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(b => new { b.UserId, b.EventType, b.CreatedAt });
    }
}
