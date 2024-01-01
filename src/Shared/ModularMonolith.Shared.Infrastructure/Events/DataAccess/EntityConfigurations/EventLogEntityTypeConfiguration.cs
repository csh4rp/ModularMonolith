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

    public EventLogEntityTypeConfiguration(bool excludeFromMigrations, string table = "event_log",
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

        builder.Property(b => b.EventName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.EventType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.OperationName)
            .IsRequired()
            .HasMaxLength(256);

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

        builder.HasIndex(b => b.PublishedAt).HasFilter("published_at IS NULL");

        builder.HasIndex(b => new { b.UserId, b.EventType, b.CreatedAt });

        builder.HasIndex(b => b.CorrelationId);
    }
}
