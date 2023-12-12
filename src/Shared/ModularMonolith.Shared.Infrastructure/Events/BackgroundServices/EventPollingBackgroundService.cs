﻿using Microsoft.EntityFrameworkCore;
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
    private readonly IOptionsMonitor<EventOptions> _optionsMonitor;
    private readonly ILogger<EventPollingBackgroundService> _logger;
    private readonly EventReader _eventReader;
    private readonly EventChannel _eventChannel;

    public EventPollingBackgroundService(IOptionsMonitor<EventOptions> optionsMonitor,
        ILogger<EventPollingBackgroundService> logger, EventReader eventReader, EventChannel eventChannel)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _eventReader = eventReader;
        _eventChannel = eventChannel;
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

    private async ValueTask RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(_optionsMonitor.CurrentValue.PollInterval);

            while (!cancellationToken.IsCancellationRequested)
            {
                var events = await _eventReader.GetUnpublishedEventsAsync(cancellationToken);

                foreach (var eventInfo in events)
                {
                    await _eventChannel.WriteAsync(eventInfo, cancellationToken);
                }
                
                await timer.WaitForNextTickAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while fetching unpublished events");
            throw;
        }
    }
}
