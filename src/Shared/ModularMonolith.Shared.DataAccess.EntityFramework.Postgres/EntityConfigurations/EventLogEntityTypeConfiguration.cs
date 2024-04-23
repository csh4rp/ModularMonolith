using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Entities;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EntityConfigurations;

public sealed class EventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLogEntity>
{
    private readonly string _schemaName;
    private readonly string _tableName;

    public EventLogEntityTypeConfiguration(string schemaName = "shared", string tableName = "event_log")
    {
        _schemaName = schemaName;
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<EventLogEntity> builder)
    {
        builder.ToTable(_tableName, _schemaName);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.EventTypeName)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(b => b.EventPayload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJson("meta_data");
        });

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EventTypeName, b.Timestamp });
    }
}
