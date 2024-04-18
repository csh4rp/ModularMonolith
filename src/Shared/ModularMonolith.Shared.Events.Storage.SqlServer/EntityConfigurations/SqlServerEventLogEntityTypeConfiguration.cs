﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.Events.Storage.SqlServer.EntityConfigurations;

public sealed class SqlServerEventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLog>
{
    private readonly string _schemaName;
    private readonly string _tableName;

    public SqlServerEventLogEntityTypeConfiguration(string schemaName = "Shared", string tableName = "EventLog")
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
            .IsRequired();

        builder.Property(b => b.EventType)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(b => new { b.Subject, b.EventType, b.OccurredAt });
    }
}
