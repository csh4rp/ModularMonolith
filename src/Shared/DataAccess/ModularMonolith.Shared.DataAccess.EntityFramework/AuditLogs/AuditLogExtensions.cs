﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;

public static class AuditLogExtensions
{
    private const string AuditIgnoreAnnotation = nameof(AuditIgnoreAnnotation);

    public static EntityTypeBuilder<TEntity> AuditIgnore<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class
    {
        entityTypeBuilder.Metadata.AddAnnotation(AuditIgnoreAnnotation, true);

        return entityTypeBuilder;
    }

    public static IMutableEntityType AuditIgnore(this IMutableEntityType entityType)
    {
        entityType.AddAnnotation(AuditIgnoreAnnotation, true);

        return entityType;
    }

    public static bool IsAuditable(this EntityEntry entry)
    {
        var annotation = entry.Metadata.FindAnnotation(AuditIgnoreAnnotation);

        return annotation?.Value is not true;
    }

    public static PropertyBuilder<TProperty> AuditIgnore<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
    {
        propertyBuilder.Metadata.AddAnnotation(AuditIgnoreAnnotation, true);

        return propertyBuilder;
    }

    public static bool IsAuditable(this PropertyEntry entry)
    {
        var annotation = entry.Metadata.FindAnnotation(AuditIgnoreAnnotation);

        return annotation?.Value is not true;
    }
}
