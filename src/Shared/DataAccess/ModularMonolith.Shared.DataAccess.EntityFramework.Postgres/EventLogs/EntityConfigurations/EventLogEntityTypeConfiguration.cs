﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.Models;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.EntityConfigurations;

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
            .HasMaxLength(512)
            .IsRequired();

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJson("meta_data");
        })
        .AuditIgnore();

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EventTypeName, b.Timestamp });
    }
}
