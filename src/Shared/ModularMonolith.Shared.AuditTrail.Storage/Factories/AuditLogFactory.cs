using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModularMonolith.Shared.Domain.ValueObjects;
using ModularMonolith.Shared.Identity;

namespace ModularMonolith.Shared.AuditTrail.Storage.Factories;

public sealed class AuditLogFactory
{
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeProvider _timeProvider;

    public AuditLogFactory(IIdentityContextAccessor identityContextAccessor,
        TimeProvider timeProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _identityContextAccessor = identityContextAccessor;
        _timeProvider = timeProvider;
        _httpContextAccessor = httpContextAccessor;
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

        var activity = Activity.Current;
        var httpContext = _httpContextAccessor.HttpContext;
        var identityContext = _identityContextAccessor.IdentityContext;

        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers.UserAgent;

        return new AuditLog
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            Subject = identityContext?.Subject,
            EntityState = GetChangeType(entry),
            EntityKeys = keys,
            EntityType = entityType.FullName!,
            EntityPropertyChanges = [.. changes],
            OperationName = Activity.Current.OperationName,
            TraceId = activity.TraceId.ToString(),
            SpanId = activity.SpanId.ToString(),
            ParentSpanId = activity.Parent is null
                ? null
                : activity.ParentSpanId.ToString(),
            IpAddress = ipAddress,
            UserAgent = userAgent
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
