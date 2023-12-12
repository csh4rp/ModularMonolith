using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Polly;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventPublisherBackgroundService : BackgroundService
{
    private readonly Task[] _tasks = new Task[Environment.ProcessorCount * 2];
    private readonly EventReader _eventReader;
    private readonly EventChannel _eventChannel;
    private readonly EventPublisher _eventPublisher;
    private readonly ILogger<EventPublisherBackgroundService> _logger;

    public EventPublisherBackgroundService(EventReader eventReader,
        EventChannel eventChannel,
        EventPublisher eventPublisher,
        ILogger<EventPublisherBackgroundService> logger)
    {
        _eventReader = eventReader;
        _eventChannel = eventChannel;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventReader.EnsureInitializedAsync(stoppingToken);
        
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            _tasks[i] = Task.Factory.StartNew(async () =>
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
                
            }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        await Task.WhenAll(_tasks);
    }

    
    private async ValueTask RunAsync(CancellationToken cancellationToken)
    {
        var publishPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(10),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3
            })
            .Build(); 
        
        var markAsPublishedPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromMinutes(2),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 10
            })
            .Build(); 
        
        await foreach (var eventInfo in _eventChannel.ReadAllAsync(cancellationToken))
        {
            try
            {
                var eventLog = await _eventReader.TryAcquireLockAsync(eventInfo, cancellationToken);
                if (eventLog is null)
                {
                    continue;
                }

                // Try to publish up to three times before releasing the lock
                await publishPipeline.ExecuteAsync(async (el, cts) => 
                    await _eventPublisher.PublishAsync(el, cts), eventLog, cancellationToken);
                
                // Try to mark as published up to ten times before going for re-publish
                await markAsPublishedPipeline.ExecuteAsync(async (ev, cts) => 
                    await _eventReader.MarkAsPublishedAsync(ev, cts), eventInfo, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while publishing events events");
                
                await _eventReader.IncrementFailedAttemptNumberAsync(eventInfo, cancellationToken);
            }
        }
    }


}
