using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ModularMonolith.Shared.Infrastructure.Events;

internal sealed class EventLogEntityTypeConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable(nameof(EventLog), "Shared");

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
            .IsRequired()
            .HasMaxLength(128);
        
        builder.Property(b => b.TraceId)
            .IsRequired()
            .HasMaxLength(32);
    }
}
