using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.EventLogs.Models;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.EventLogs.EntityConfigurations;

public sealed class EventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLogEntity>
{
    private readonly string _containerName;

    public EventLogEntityTypeConfiguration(string containerName = "EventLogs") => _containerName = containerName;

    public void Configure(EntityTypeBuilder<EventLogEntity> builder)
    {
        builder.ToContainer(_containerName);

        builder.HasNoDiscriminator();

        builder.HasKey(b => b.Id);

        builder.HasPartitionKey(b => b.PartitionKey);

        builder.Property(b => b.Id)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        builder.Property(b => b.EventTypeName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(b => b.EventPayload)
            .IsRequired();

        builder.OwnsOne(b => b.MetaData, b =>
        {
            b.ToJsonProperty(nameof(EventLogEntity.MetaData));
            b.Property(p => p.IpAddress).ToJsonProperty(nameof(EventLogEntityMetaData.IpAddress));
            b.Property(p => p.OperationName).ToJsonProperty(nameof(EventLogEntityMetaData.OperationName));
            b.Property(p => p.ParentSpanId).ToJsonProperty(nameof(EventLogEntityMetaData.ParentSpanId));
            b.Property(p => p.SpanId).ToJsonProperty(nameof(EventLogEntityMetaData.SpanId));
            b.Property(p => p.TraceId).ToJsonProperty(nameof(EventLogEntityMetaData.TraceId));
            b.Property(p => p.Subject).ToJsonProperty(nameof(EventLogEntityMetaData.Subject));
            b.Property(p => p.Uri).ToJsonProperty(nameof(EventLogEntityMetaData.Uri));
        });

        builder.HasIndex(b => b.Timestamp);

        builder.HasIndex(b => new { b.EventTypeName, b.Timestamp });
    }
}
