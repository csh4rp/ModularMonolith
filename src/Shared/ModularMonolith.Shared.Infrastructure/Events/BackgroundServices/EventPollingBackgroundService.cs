using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Polly;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventPollingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly EventChannel _eventChannel;
    private readonly IOptionsMonitor<EventOptions> _optionsMonitor;
    private readonly ILogger<EventPollingBackgroundService> _logger;

    public EventPollingBackgroundService(IServiceScopeFactory serviceScopeFactory,
        EventChannel eventChannel,
        IOptionsMonitor<EventOptions> optionsMonitor,
        ILogger<EventPollingBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _eventChannel = eventChannel;
        _optionsMonitor = optionsMonitor;
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
            using var timer = new PeriodicTimer(_optionsMonitor.CurrentValue.PollInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                await using (var scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    await using var eventLogContext = scope.ServiceProvider.GetRequiredService<IEventLogContext>();

                    var ids = await eventLogContext.EventLogs
                        .Where(e => e.PublishedAt == null)
                        .Select(e => e.Id)
                        .ToListAsync(stoppingToken);

                    foreach (var id in ids)
                    {
                        await _eventChannel.WriteAsync(id, stoppingToken);
                    }
                }

                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while fetching unpublished events");
            throw;
        }
    }
}
