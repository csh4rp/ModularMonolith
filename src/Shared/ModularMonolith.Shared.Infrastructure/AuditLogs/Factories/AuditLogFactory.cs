using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Domain.ValueObjects;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Extensions;
using EntityState = ModularMonolith.Shared.Domain.Enums.EntityState;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs.Factories;

internal sealed class AuditLogFactory
{
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly TimeProvider _timeProvider;

    public AuditLogFactory(IIdentityContextAccessor identityContextAccessor, TimeProvider timeProvider)
    {
        _identityContextAccessor = identityContextAccessor;
        _timeProvider = timeProvider;
    }

    public AuditLog Create(EntityEntry entry)
    {
        Debug.Assert(Activity.Current is not null);

        var entityType = entry.Entity.GetType();

        var auditableProperties = entry.Properties.Where(p => p.IsAuditable())
            .ToList();

        var changes = new List<PropertyChange>();

        var changeType = GetChangeType(entry);

        foreach (var propertyEntry in auditableProperties)
        {
            if (propertyEntry.Metadata.IsPrimaryKey())
            {
                continue;
            }

            if (changeType == EntityState.Modified && propertyEntry.IsModified)
            {
                var change = new PropertyChange(propertyEntry.Metadata.Name, propertyEntry.CurrentValue,
                    propertyEntry.OriginalValue);
                changes.Add(change);
            }
            else
            {
                var change = new PropertyChange(propertyEntry.Metadata.Name, propertyEntry.CurrentValue, null);
                changes.Add(change);
            }
        }

        var keys = new List<EntityKey>();

        var primaryKey = entry.Metadata.GetKeys().Single(k => k.IsPrimaryKey());

        foreach (var primaryKeyProperty in primaryKey.Properties)
        {
            var propertyEntry = auditableProperties.Single(p => p.Metadata.Name == primaryKeyProperty.Name);

            Debug.Assert(propertyEntry.CurrentValue is not null);

            keys.Add(new EntityKey(propertyEntry.Metadata.Name, propertyEntry.CurrentValue));
        }

        return new AuditLog
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            EntityState = GetChangeType(entry),
            ActivityId = Activity.Current.TraceId.ToString(),
            UserId = _identityContextAccessor.Context?.UserId,
            OperationName = Activity.Current.OperationName,
            EntityType = entityType.FullName!,
            EntityPropertyChanges = [.. changes],
            EntityKeys = keys
        };
    }

    private static EntityState GetChangeType(EntityEntry entry) => entry.State switch
    {
        Microsoft.EntityFrameworkCore.EntityState.Added => EntityState.Added,
        Microsoft.EntityFrameworkCore.EntityState.Modified => EntityState.Modified,
        Microsoft.EntityFrameworkCore.EntityState.Deleted => EntityState.Deleted,
        _ => throw new ArgumentOutOfRangeException()
    };
}
