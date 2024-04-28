namespace ModularMonolith.Shared.Messaging.MassTransit;

public static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 10000, Level = LogLevel.Debug, Message = "Persisted event of type: '{EventType}' and id: '{EventId}'")]
    public static partial void EventPersisted(this ILogger logger, string eventType, Guid eventId);

    [LoggerMessage(EventId = 10001, Level = LogLevel.Debug, Message = "Persisted events with ids: {EventIds}")]
    public static partial void EventsPersisted(this ILogger logger, IEnumerable<Guid> eventIds);

    [LoggerMessage(EventId = 10002, Level = LogLevel.Debug, Message = "Skipped event persistence for event of type: '{EventType}' and id: '{EventId}'")]
    public static partial void EventPersistenceSkipped(this ILogger logger, string eventType, Guid eventId);

    [LoggerMessage(EventId = 10003, Level = LogLevel.Debug, Message = "Published event of type: '{EventType}' and id: '{EventId}'")]
    public static partial void EventPublished(this ILogger logger, string eventType, Guid eventId);
}
