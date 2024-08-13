using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModularMonolith.Shared.DataAccess.AudiLogs;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Factories;

public sealed class AuditLogFactory
{
    private readonly IAuditMetaDataProvider _auditMetaDataProvider;
    private readonly TimeProvider _timeProvider;

    public AuditLogFactory(IAuditMetaDataProvider auditMetaDataProvider, TimeProvider timeProvider)
    {
        _auditMetaDataProvider = auditMetaDataProvider;
        _timeProvider = timeProvider;
    }

    public AuditLogEntry Create(EntityEntry entry)
    {
        Debug.Assert(Activity.Current is not null);

        var entityType = entry.Entity.GetType();

        var auditableProperties = entry.Properties.Where(p => p.IsAuditable())
            .ToList();

        var changes = new List<EntityFieldChange>();
        var keyFields = new List<EntityField>();
        var primaryKey = entry.Metadata.GetKeys().Single(k => k.IsPrimaryKey());
        var operationType = GetOperationType(entry);

        foreach (var propertyEntry in auditableProperties)
        {
            if (propertyEntry.Metadata.IsPrimaryKey())
            {
                continue;
            }

            if (operationType == AuditOperationType.Modified && propertyEntry.IsModified)
            {
                var change = new EntityFieldChange(propertyEntry.Metadata.Name,
                    propertyEntry.OriginalValue?.ToString(),
                    propertyEntry.CurrentValue?.ToString());

                changes.Add(change);
            }
            else
            {
                var change = new EntityFieldChange(propertyEntry.Metadata.Name, null,
                    propertyEntry.CurrentValue?.ToString());
                changes.Add(change);
            }
        }

        foreach (var primaryKeyProperty in primaryKey.Properties)
        {
            var propertyEntry = auditableProperties.Single(p => p.Metadata.Name == primaryKeyProperty.Name);

            Debug.Assert(propertyEntry.CurrentValue is not null);

            keyFields.Add(new EntityField(propertyEntry.Metadata.Name, propertyEntry.CurrentValue?.ToString()));
        }

        return new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = _timeProvider.GetUtcNow(),
            OperationType = GetOperationType(entry),
            EntityKey = new EntityKey(keyFields),
            EntityType = entityType,
            EntityChanges = new EntityChanges(changes),
            MetaData = _auditMetaDataProvider.GetMetaData()
        };
    }

    private static AuditOperationType GetOperationType(EntityEntry entry) => entry.State switch
    {
        EntityState.Added => AuditOperationType.Added,
        EntityState.Modified => AuditOperationType.Modified,
        EntityState.Deleted => AuditOperationType.Deleted,
        _ => throw new ArgumentOutOfRangeException()
    };
}
