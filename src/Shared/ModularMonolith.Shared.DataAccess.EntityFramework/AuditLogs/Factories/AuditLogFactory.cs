using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModularMonolith.Shared.DataAccess.AudiLogs;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Factories;

public sealed class AuditLogFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeProvider _timeProvider;

    public AuditLogFactory(IHttpContextAccessor httpContextAccessor, TimeProvider timeProvider)
    {
        _httpContextAccessor = httpContextAccessor;
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
                    propertyEntry.OriginalValue,
                    propertyEntry.CurrentValue);

                changes.Add(change);
            }
            else
            {
                var change = new EntityFieldChange(propertyEntry.Metadata.Name, null, propertyEntry.CurrentValue);
                changes.Add(change);
            }
        }


        foreach (var primaryKeyProperty in primaryKey.Properties)
        {
            var propertyEntry = auditableProperties.Single(p => p.Metadata.Name == primaryKeyProperty.Name);

            Debug.Assert(propertyEntry.CurrentValue is not null);

            keyFields.Add(new EntityField(propertyEntry.Metadata.Name, propertyEntry.CurrentValue));
        }

        var activity = Activity.Current;
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress;
        var subject = httpContext?.User.Identity?.Name;
        var uri = httpContext?.Request.GetDisplayUrl();

        return new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = _timeProvider.GetUtcNow(),
            OperationType = GetOperationType(entry),
            EntityKey = new EntityKey(keyFields),
            EntityType = entityType,
            EntityChanges = new EntityChanges(changes),
            MetaData = new AuditMetaData
            {
                Subject = subject,
                OperationName = activity.OperationName,
                TraceId = activity.TraceId,
                SpanId = activity.SpanId,
                ParentSpanId = activity.Parent is null ? null : activity.ParentSpanId,
                IpAddress = ipAddress,
                Uri = uri is null ? null : new Uri(uri)
            }
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
