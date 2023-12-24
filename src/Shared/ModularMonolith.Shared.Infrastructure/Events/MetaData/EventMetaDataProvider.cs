using Microsoft.EntityFrameworkCore;
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
        using var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventLog))!;

        return new EventLogMetaData
        {
            TableName = entity.GetTableName()!,
            IdColumnName = entity.FindProperty(nameof(EventLog.Id))!.GetColumnName(),
            NameColumnName = entity.FindProperty(nameof(EventLog.Name))!.GetColumnName(),
            OperationNameColumnName = entity.FindProperty(nameof(EventLog.OperationName))!.GetColumnName(),
            PayloadColumnName = entity.FindProperty(nameof(EventLog.Payload))!.GetColumnName(),
            PublishedAtColumnName = entity.FindProperty(nameof(EventLog.PublishedAt))!.GetColumnName(),
            TypeColumnName = entity.FindProperty(nameof(EventLog.Type))!.GetColumnName(),
            ActivityIdColumnName = entity.FindProperty(nameof(EventLog.ActivityId))!.GetColumnName(),
            CorrelationIdColumnName = entity.FindProperty(nameof(EventLog.CorrelationId))!.GetColumnName(),
            CreatedAtColumnName = entity.FindProperty(nameof(EventLog.CreatedAt))!.GetColumnName(),
            UserIdColumnName = entity.FindProperty(nameof(EventLog.UserId))!.GetColumnName(),
            IpAddressColumnName = entity.FindProperty(nameof(EventLog.IpAddress))!.GetColumnName(),
            UserAgentColumnName = entity.FindProperty(nameof(EventLog.UserAgent))!.GetColumnName()
        };
    }

    private static EventLogLockMetaData CreateEventLockMetaData(IServiceProvider serviceProvider)
    {
        using var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventLogLock))!;

        return new EventLogLockMetaData
        {
            TableName = entity.GetTableName()!,
            IdColumnName = entity.FindProperty(nameof(EventLogLock.EventLogId))!.GetColumnName(),
            AcquiredAtColumnName = entity.FindProperty(nameof(EventLogLock.AcquiredAt))!.GetColumnName()
        };
    }

    private static EventLogCorrelationLockMetaData CreateEventCorrelationLockMetaData(IServiceProvider serviceProvider)
    {
        using var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventCorrelationLock))!;

        return new EventLogCorrelationLockMetaData
        {
            TableName = entity.GetTableName()!,
            CorrelationIdColumnName =
                entity.FindProperty(nameof(EventCorrelationLock.CorrelationId))!.GetColumnName(),
            AcquiredAtColumnName = entity.FindProperty(nameof(EventCorrelationLock.AcquiredAt))!.GetColumnName()
        };
    }
    
    private static EventLogPublishAttemptMetaData CreateEventLogPublishAttemptMetaData(IServiceProvider serviceProvider)
    {
        using var dbContext = serviceProvider.GetRequiredService<IEventLogDbContext>();
        var model = dbContext.Model;

        var entity = model.FindEntityType(typeof(EventLogPublishAttempt))!;

        return new EventLogPublishAttemptMetaData
        {
            TableName = entity.GetTableName()!,
            EventLogIdColumnName = entity.FindProperty(nameof(EventLogPublishAttempt.EventLogId))!.GetColumnName(),
            AttemptNumberColumnName = entity.FindProperty(nameof(EventLogPublishAttempt.AttemptNumber))!.GetColumnName(),
            NextAttemptAtColumnName = entity.FindProperty(nameof(EventLogPublishAttempt.NextAttemptAt))!.GetColumnName()
        };
    }
}
