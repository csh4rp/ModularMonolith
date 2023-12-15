using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal sealed class EventCorrelationLockEntityTypeConfiguration : IEntityTypeConfiguration<EventCorrelationLock>
{
    private readonly string _table;
    private readonly string? _schema;

    public EventCorrelationLockEntityTypeConfiguration(string table = nameof(EventCorrelationLock), string? schema = null)
    {
        _table = table;
        _schema = schema;
    }

    public void Configure(EntityTypeBuilder<EventCorrelationLock> builder)
    {
        builder.ToTable(_table, _schema);

        builder.HasKey(b => b.CorrelationId);
    }
}
