namespace ModularMonolith.Shared.Infrastructure.Events.Extensions;

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Debug,
        Message = "Received Notification from database with id: {EventLogId} and correlationId: {CorrelationId}")]
    public static partial void NotificationReceived(this ILogger logger, Guid eventLogId, Guid? correlationId);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message =
            "Notification with id: {EventLogId} and correlationId: {CorrelationId} was blocked due to publication channel being full")]
    public static partial void NotificationBlocked(this ILogger logger, Guid eventLogId, Guid? correlationId);

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Warning,
        Message = "Event with id: {EventId} was already taken by other worker")]
    public static partial void EventAlreadyTaken(this ILogger logger, Guid eventId);

    [LoggerMessage(
        EventId = 2020,
        Level = LogLevel.Debug,
        Message = "Event with id: {EventId} was published")]
    public static partial void EventPublished(this ILogger logger, Guid eventId);

    [LoggerMessage(
        EventId = 2021,
        Level = LogLevel.Debug,
        Message = "IntegrationEvent with id: {EventId} was published")]
    public static partial void IntegrationEventPublished(this ILogger logger, Guid eventId);

    [LoggerMessage(
        EventId = 2030,
        Level = LogLevel.Debug,
        Message = "Awaiting for next event to arrive")]
    public static partial void AwaitingNextEvent(this ILogger logger);
}
