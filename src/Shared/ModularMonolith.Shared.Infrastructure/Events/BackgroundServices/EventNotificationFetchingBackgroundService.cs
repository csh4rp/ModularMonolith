using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Npgsql;
using Polly;
using Polly.Registry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventNotificationFetchingBackgroundService : BackgroundService
{
    private readonly ResiliencePipeline _retryPipeline;
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly IEventChannel _channel;
    private readonly ILogger<EventNotificationFetchingBackgroundService> _logger;

    public EventNotificationFetchingBackgroundService(ResiliencePipelineProvider<string> pipelineProvider,
        DbConnectionFactory dbConnectionFactory,
        IEventChannel channel,
        ILogger<EventNotificationFetchingBackgroundService> logger)
    {
        _retryPipeline = pipelineProvider.GetPipeline(EventConstants.EventNotificationFetchingPipelineName);
        _dbConnectionFactory = dbConnectionFactory;
        _channel = channel;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await _retryPipeline.ExecuteAsync(RunAsync, stoppingToken);

    private async ValueTask RunAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Started listening for the notifications");

        try
        {
            await using var connection = _dbConnectionFactory.Create();
            await connection.OpenAsync(stoppingToken);

            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "LISTEN event_log_queue;";
                await cmd.ExecuteNonQueryAsync(stoppingToken);
            }

            connection.Notification += OnNotificationEventHandler;

            while (!stoppingToken.IsCancellationRequested)
            {
                await connection.WaitAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching new event ids");
            throw;
        }
    }

    private void OnNotificationEventHandler(object obj, NpgsqlNotificationEventArgs args)
    {
        try
        {
            var (id, correlationId) = GetIds(args);

            var eventInfo = new EventInfo(id, correlationId);

            _logger.EventLogReceived(id, correlationId);

            var spinWait = new SpinWait();
            
            while (!_channel.Writer.TryWrite(eventInfo))
            {
                if (spinWait.NextSpinWillYield)
                {
                    _logger.EventLogBlocked(id, correlationId);
                }

                spinWait.SpinOnce();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An error occurred while writing event with notification payload: {NotificationPayload} to channel",
                args.Payload);
        }
    }

    private static (Guid Id, Guid? CorrelationId) GetIds(NpgsqlNotificationEventArgs args)
    {
        var index = args.Payload.IndexOf('/');
        var payload = args.Payload.AsSpan();

        Guid? correlationId = index != args.Payload.Length - 1
            ? Guid.Parse(payload[(index + 1)..])
            : null;

        var id = Guid.Parse(payload[..index]);
        return (id, correlationId);
    }
}
