using Microsoft.Extensions.Logging;

namespace ModularMonolith.Shared.BusinessLogic.Middlewares;

internal static class LoggingHelpers
{
    private static readonly Action<ILogger, string, Exception?> OperationStartedCallback = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(0, "Operation Started"),
        "Operation: '{OperationType}' has started");
    
    private static readonly Action<ILogger, string, Exception?> OperationFinishedCallback = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(1, "Operation Finished"),
        "Operation: '{OperationType}' has finished");

    public static void OperationStarted(this ILogger logger, string name) => OperationStartedCallback(logger, name, null);
    
    public static void OperationFinished(this ILogger logger, string name) => OperationFinishedCallback(logger, name, null);
}
