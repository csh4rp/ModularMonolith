using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal sealed class EventLogLockEntityTypeConfiguration : IEntityTypeConfiguration<EventLogLock>
{
    private readonly string _table;
    private readonly string? _schema;

    public EventLogLockEntityTypeConfiguration(string table = nameof(EventLogLock), string? schema = null)
    {
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<EventLogLock> builder)
    {
        builder.ToTable(_table, _schema);

        builder.HasKey(b => b.EventLogId);
    }
}
