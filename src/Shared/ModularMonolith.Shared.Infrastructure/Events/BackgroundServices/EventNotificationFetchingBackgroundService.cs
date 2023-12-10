using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Npgsql;
using Polly;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventNotificationFetchingBackgroundService : BackgroundService
{
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Constant,
                MaxRetryAttempts = int.MaxValue
            })
            .Build();

        await pipeline.ExecuteAsync(RunAsync, stoppingToken);
    }

    private async ValueTask RunAsync(CancellationToken stoppingToken)
    {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching new event ids");
            throw;
        }
    }
    
    private void OnNotificationEventHandler(object obj, NpgsqlNotificationEventArgs args)
    {
        var index = args.Payload.IndexOf('/');
        var payload = args.Payload.AsSpan();

        Guid? correlationId = index != 0
            ? Guid.Parse(payload[..index])
            : null;
        
        var id = Guid.Parse(payload[index..]);

        var eventInfo = new EventInfo(id, correlationId);

        var spin = new SpinWait();

        while (!_channel.TryWrite(eventInfo))
        {
            spin.SpinOnce();
        }
    }
}
