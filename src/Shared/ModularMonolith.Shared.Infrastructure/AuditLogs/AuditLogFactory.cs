using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Core;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public class AuditLogFactory
{
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditLogFactory(IIdentityContextAccessor identityContextAccessor, IDateTimeProvider dateTimeProvider)
    {
        _identityContextAccessor = identityContextAccessor;
        _dateTimeProvider = dateTimeProvider;
    }

    public AuditLog Create(EntityEntry entry)
    {
        Debug.Assert(Activity.Current is not null);
        
        var entityType = entry.Entity.GetType();

        var auditableProperties = entry.Properties.Where(p => p.IsAuditable())
            .ToList();

        var changes = new Dictionary<string, PropertyChange>();

        var changeType = GetChangeType(entry);
        
        foreach (var propertyEntry in auditableProperties)
        {
            if (propertyEntry.Metadata.IsPrimaryKey())
            {
                continue;
            }
            
            if (changeType == ChangeType.Modified && propertyEntry.IsModified)
            {
                var change = new PropertyChange(propertyEntry.CurrentValue, propertyEntry.OriginalValue);
                changes.Add(propertyEntry.Metadata.Name, change);
            }
            else
            {
                var change = new PropertyChange(propertyEntry.CurrentValue, null);
                changes.Add(propertyEntry.Metadata.Name, change);
            }
        }
        
        var keys = new Dictionary<string, object>();

        var primaryKey = entry.Metadata.GetKeys().Single(k => k.IsPrimaryKey());

        foreach (var primaryKeyProperty in primaryKey.Properties)
        {
            var propertyEntry = auditableProperties.Single(p => p.Metadata.Name == primaryKeyProperty.Name);
         
            Debug.Assert(propertyEntry.CurrentValue is not null);
            keys.Add(propertyEntry.Metadata.Name, propertyEntry.CurrentValue);
        }
        
        return new AuditLog
        {
            CreatedAt = _dateTimeProvider.GetUtcNow(),
            ChangeType = GetChangeType(entry),
            TraceId = Activity.Current.TraceId.ToString(),
            UserId = _identityContextAccessor.Context?.UserId,
            OperationName = Activity.Current.OperationName,
            EntityType = entityType.FullName!,
            Changes = JsonSerializer.Serialize(changes),
            EntityKeys = JsonSerializer.Serialize(keys)
        };
    }

    private static ChangeType GetChangeType(EntityEntry entry) => entry.State switch
    {
        EntityState.Added => ChangeType.Added,
        EntityState.Modified => ChangeType.Modified,
        EntityState.Deleted => ChangeType.Deleted,
        _ => throw new ArgumentOutOfRangeException()
    };
}
