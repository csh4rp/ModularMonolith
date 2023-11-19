using Microsoft.Extensions.Logging;

namespace ModularMonolith.Shared.Infrastructure.Events.Extensions;

internal static partial class EventLoggingExtensions
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Events with ids: [{EventIds}] were marked as published")]
    public static partial void EventsMarkedAsPublished(this ILogger logger, List<Guid> eventIds);
    
    [LoggerMessage(EventId = 2001, Level = LogLevel.Debug, Message = "No unpublished events found in the database")]
    public static partial void NoNewEvents(this ILogger logger);
    
    [LoggerMessage(EventId = 2002, Level = LogLevel.Debug, Message = "Fetched: {EventCount} unpublished events")]
    public static partial void FetchedNewEvents(this ILogger logger, int eventCount);
    
    [LoggerMessage(EventId = 2003, Level = LogLevel.Debug, Message = "Event with id: {EventId} was published to message bus")]
    public static partial void EventPublished(this ILogger logger, Guid eventId);
    
    [LoggerMessage(EventId = 2004, Level = LogLevel.Debug, Message = "IntegrationEvent with id: {EventId} was published to message bus")]
    public static partial void IntegrationEventPublished(this ILogger logger, Guid eventId);
}
