using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

internal sealed class EventMetaDataProvider
{
    public EventMetaDataProvider(IServiceProvider serviceProvider)
    {
        EventLogMetaData = CreateEventLogMetaData(serviceProvider);
        EventLogLockMetaData = CreateEventLockMetaData(serviceProvider);
        EventLogCorrelationLockMetaData = CreateEventCorrelationLockMetaData(serviceProvider);
        EventLogPublishAttemptMetaData = CreateEventLogPublishAttemptMetaData(serviceProvider);
    }

    public EventLogMetaData EventLogMetaData { get; }

    public EventLogLockMetaData EventLogLockMetaData { get; }

    public EventLogCorrelationLockMetaData EventLogCorrelationLockMetaData { get; }

    public EventLogPublishAttemptMetaData EventLogPublishAttemptMetaData { get; }

    private static EventLogMetaData CreateEventLogMetaData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventLog))!;

        return new EventLogMetaData
        {
            TableName = GetFullTableName(entity),
            IdColumnName = entity.FindProperty(nameof(EventLog.Id))!.GetColumnName(),
            EventNameColumnName = entity.FindProperty(nameof(EventLog.EventName))!.GetColumnName(),
            OperationNameColumnName = entity.FindProperty(nameof(EventLog.OperationName))!.GetColumnName(),
            EventPayloadColumnName = entity.FindProperty(nameof(EventLog.EventPayload))!.GetColumnName(),
            PublishedAtColumnName = entity.FindProperty(nameof(EventLog.PublishedAt))!.GetColumnName(),
            EventTypeColumnName = entity.FindProperty(nameof(EventLog.EventType))!.GetColumnName(),
            TraceIdColumnName = entity.FindProperty(nameof(EventLog.TraceId))!.GetColumnName(),
            SpanIdColumnName = entity.FindProperty(nameof(EventLog.SpanId))!.GetColumnName(),
            ParentSpanIdColumnName = entity.FindProperty(nameof(EventLog.ParentSpanId))!.GetColumnName(),
            CorrelationIdColumnName = entity.FindProperty(nameof(EventLog.CorrelationId))!.GetColumnName(),
            CreatedAtColumnName = entity.FindProperty(nameof(EventLog.CreatedAt))!.GetColumnName(),
            UserIdColumnName = entity.FindProperty(nameof(EventLog.UserId))!.GetColumnName(),
            UserNameColumnName = entity.FindProperty(nameof(EventLog.UserName))!.GetColumnName(),
            IpAddressColumnName = entity.FindProperty(nameof(EventLog.IpAddress))!.GetColumnName(),
            UserAgentColumnName = entity.FindProperty(nameof(EventLog.UserAgent))!.GetColumnName()
        };
    }

    private static EventLogLockMetaData CreateEventLockMetaData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventLogLock))!;

        return new EventLogLockMetaData
        {
            TableName = GetFullTableName(entity),
            IdColumnName = entity.FindProperty(nameof(EventLogLock.EventLogId))!.GetColumnName(),
            AcquiredAtColumnName = entity.FindProperty(nameof(EventLogLock.AcquiredAt))!.GetColumnName()
        };
    }

    private static EventLogCorrelationLockMetaData CreateEventCorrelationLockMetaData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventCorrelationLock))!;
        
        return new EventLogCorrelationLockMetaData
        {
            TableName = GetFullTableName(entity),
            CorrelationIdColumnName =
                entity.FindProperty(nameof(EventCorrelationLock.CorrelationId))!.GetColumnName(),
            AcquiredAtColumnName = entity.FindProperty(nameof(EventCorrelationLock.AcquiredAt))!.GetColumnName()
        };
    }

    private static EventLogPublishAttemptMetaData CreateEventLogPublishAttemptMetaData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventLogPublishAttempt))!;

        return new EventLogPublishAttemptMetaData
        {
            TableName = GetFullTableName(entity),
            EventLogIdColumnName = entity.FindProperty(nameof(EventLogPublishAttempt.EventLogId))!.GetColumnName(),
            AttemptNumberColumnName =
                entity.FindProperty(nameof(EventLogPublishAttempt.AttemptNumber))!.GetColumnName(),
            NextAttemptAtColumnName =
                entity.FindProperty(nameof(EventLogPublishAttempt.NextAttemptAt))!.GetColumnName()
        };
    }

    private static string GetFullTableName(IEntityType entityType)
    {
        var schema = entityType.GetSchema();
        var table = entityType.GetTableName()!;
        
        return string.IsNullOrEmpty(schema) ? table : $"{schema}.{table}";
    }
}
