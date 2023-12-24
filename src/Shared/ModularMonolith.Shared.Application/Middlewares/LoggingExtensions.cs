using Microsoft.Extensions.Logging;

namespace ModularMonolith.Shared.Application.Middlewares;

internal static partial class LoggingExtensions
{
    [LoggerMessage(EventId = 3000, Level = LogLevel.Information, Message = "Operation: '{OperationType}' has started")]
    public static partial void OperationStarted(this ILogger logger, string operationType);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Operation: '{OperationType}' has finished")]
    public static partial void OperationFinished(this ILogger logger, string operationType);
}
