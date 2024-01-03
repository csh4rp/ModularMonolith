using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Polly;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventPublisherBackgroundService : BackgroundService
{
    private static readonly ResiliencePipeline EventReceiverPipeline = CreateReceiverPipeline();
    private static readonly ResiliencePipeline EventLockReleasePipeline = CreateLockReleasePipeline();
    private static readonly ResiliencePipeline EventPublicationPipeline = CreatePublicationPipeline();
    
    private readonly Task[] _tasks;
    private readonly IEventReader _eventReader;
    private readonly EventChannel _eventChannel;
    private readonly EventPublisher _eventPublisher;
    private readonly ILogger<EventPublisherBackgroundService> _logger;

    public EventPublisherBackgroundService(IEventReader eventReader,
        EventChannel eventChannel,
        EventPublisher eventPublisher,
        ILogger<EventPublisherBackgroundService> logger,
        IOptionsMonitor<EventOptions> options)
    {
        _eventReader = eventReader;
        _eventChannel = eventChannel;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _tasks = new Task[options.CurrentValue.MaxParallelWorkers];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            _tasks[i] = Task.Factory.StartNew(async () =>
            {
                await EventReceiverPipeline.ExecuteAsync(RunAsync, stoppingToken);
            }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        await Task.WhenAll(_tasks);
    }

    private static ResiliencePipeline CreateReceiverPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Constant,
                MaxRetryAttempts = int.MaxValue
            })
            .Build();

    private static ResiliencePipeline CreateLockReleasePipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromMinutes(1), BackoffType = DelayBackoffType.Exponential, MaxRetryAttempts = 10
            })
            .Build();

    private static ResiliencePipeline CreatePublicationPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(10), BackoffType = DelayBackoffType.Exponential, MaxRetryAttempts = 3
            })
            .Build();

    private async ValueTask RunAsync(CancellationToken cancellationToken)
    {
        await foreach (var eventInfo in _eventChannel.ReadAllAsync(cancellationToken))
        {
            try
            {
                var (wasAcquired, eventLog) = await _eventReader.TryAcquireLockAsync(eventInfo, cancellationToken);
                
                if (!wasAcquired)
                {
                    _logger.EventAlreadyTaken(eventInfo.EventLogId);
                    continue;
                }

                // Try to publish up to three times before releasing the lock
                await EventPublicationPipeline.ExecuteAsync(async (el, cts) =>
                    await _eventPublisher.PublishAsync(el, cts), eventLog!, cancellationToken);

                // Try to mark as published up to ten times before going for re-publish
                await EventLockReleasePipeline.ExecuteAsync(async (ev, cts) =>
                    await _eventReader.MarkAsPublishedAsync(ev, cts), eventInfo, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while publishing events events");

                // Make sure to release the lock
                await EventLockReleasePipeline.ExecuteAsync(
                    async (ev, cts) => await _eventReader.IncrementFailedAttemptNumberAsync(ev, cts),
                    eventInfo, cancellationToken);
            }
        }
    }
}
