using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

internal sealed class EventLogPublishAttemptEntityTypeConfiguration : IEntityTypeConfiguration<EventLogPublishAttempt>
{
    private readonly bool _excludeFromMigrations;
    private readonly string _table;
    private readonly string? _schema;

    public EventLogPublishAttemptEntityTypeConfiguration(bool excludeFromMigrations,
        string table = "event_log_publish_attempt",
        string? schema = null)
    {
        _excludeFromMigrations = excludeFromMigrations;
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<EventLogPublishAttempt> builder)
    {
        if (_excludeFromMigrations)
        {
            builder.ToTable(_table, _schema, t => t.ExcludeFromMigrations());
        }
        else
        {
            builder.ToTable(_table, _schema);
        }

        builder.HasKey(b => new { b.EventLogId, b.AttemptNumber });
    }
}
