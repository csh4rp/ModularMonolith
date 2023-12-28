using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

public sealed class EventCorrelationLockEntityTypeConfiguration : IEntityTypeConfiguration<EventCorrelationLock>
{
    private readonly bool _excludeFromMigrations;
    private readonly string _table;
    private readonly string? _schema;

    public EventCorrelationLockEntityTypeConfiguration(bool excludeFromMigrations,
        string table = "event_correlation_lock", string? schema = null)
    {
        _excludeFromMigrations = excludeFromMigrations;
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<EventCorrelationLock> builder)
    {
        if (_excludeFromMigrations)
        {
            builder.ToTable(_table, _schema, t => t.ExcludeFromMigrations());
        }
        else
        {
            builder.ToTable(_table, _schema);
        }

        builder.HasKey(b => b.CorrelationId);
    }
}
