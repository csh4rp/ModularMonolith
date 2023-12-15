using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Npgsql;
using Polly;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventNotificationFetchingBackgroundService : BackgroundService
{
    private static readonly ResiliencePipeline RetryPipeline = CreatePipeline();
    
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly EventChannel _channel;
    private readonly ILogger<EventNotificationFetchingBackgroundService> _logger;

    public EventNotificationFetchingBackgroundService(DbConnectionFactory dbConnectionFactory,
        EventChannel channel,
        ILogger<EventNotificationFetchingBackgroundService> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _channel = channel;
        _logger = logger;
    }

    private static ResiliencePipeline CreatePipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Constant,
                MaxRetryAttempts = int.MaxValue
            })
            .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RetryPipeline.ExecuteAsync(RunAsync, stoppingToken);
    }

    private async ValueTask RunAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Started listening for the notifications");
        
        try
        {
            await using var connection = _dbConnectionFactory.Create();
            await connection.OpenAsync(stoppingToken);

            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "LISTEN new_events";
                await cmd.ExecuteNonQueryAsync(stoppingToken);
            }

            connection.Notification += OnNotificationEventHandler;
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await connection.WaitAsync(stoppingToken);
            }

            connection.Notification -= OnNotificationEventHandler;
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

            _logger.NotificationReceived(id, correlationId);

            if (_channel.TryWrite(eventInfo))
            {
                return;
            }

            _logger.NotificationBlocked(id, correlationId);

            var spinWait = new SpinWait();

            while (!_channel.TryWrite(eventInfo))
            {
                if (spinWait.NextSpinWillYield)
                {
                    _logger.NotificationBlocked(id, correlationId);
                }
                
                spinWait.SpinOnce();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while writing event with notification payload: {NotificationPayload} to channel",
                args.Payload);
        }
    }

    private static (Guid Id, Guid? CorrelationId) GetIds(NpgsqlNotificationEventArgs args)
    {
        var index = args.Payload.IndexOf('/');
        var payload = args.Payload.AsSpan();

        Guid? correlationId = index != 0
            ? Guid.Parse(payload[..index])
            : null;
        
        var id = Guid.Parse(payload[index..]);
        return (id, correlationId);
    }
}
