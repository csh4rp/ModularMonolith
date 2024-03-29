﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.Domain.Entities;
using EntityState = ModularMonolith.Shared.Domain.Enums.EntityState;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs.EntityConfigurations;

public sealed class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log", "shared");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.EntityState)
            .HasConversion(b => b.ToString(), b => Enum.Parse<EntityState>(b))
            .IsRequired()
            .HasMaxLength(8);

        builder.OwnsMany(b => b.EntityPropertyChanges, b =>
        {
            b.ToTable("audit_log", "shared");
            b.ToJson("entity_property_changes");
        });

        builder.OwnsMany(b => b.EntityKeys, b =>
        {
            b.ToTable("audit_log", "shared");
            b.ToJson("entity_keys");
        });

        builder.Property(b => b.EntityType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.OperationName)
            .IsRequired()
            .HasMaxLength(128);

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

        // TODO: Add index for EntityKey, EntityType [{"PropertyName": "id", "Value": "GUID"}]
    }
}
