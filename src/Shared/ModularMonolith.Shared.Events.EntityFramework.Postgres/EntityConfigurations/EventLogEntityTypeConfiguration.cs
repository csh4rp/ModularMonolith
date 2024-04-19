using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.Events.EntityFramework.Postgres.EntityConfigurations;

public sealed class EventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLog>
{
    private readonly string _schemaName;
    private readonly string _tableName;

    public EventLogEntityTypeConfiguration(string schemaName = "shared", string tableName = "event_log")
    {
        _schemaName = schemaName;
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable(_tableName, _schemaName);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.Subject)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.EventPayload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(b => b.EventType)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(b => new { b.Subject, b.EventType, b.OccurredAt });
    }
}
