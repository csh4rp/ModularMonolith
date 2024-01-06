using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Polly;
using Polly.Registry;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventPublisherBackgroundService : BackgroundService
{
    private readonly ResiliencePipeline _receiverPipeline;
    private readonly ResiliencePipeline _lockReleasePipeline;
    private readonly ResiliencePipeline _publicationPipeline;
    private readonly Task[] _tasks;
    private readonly IEventStore _eventStore;
    private readonly IEventChannel _eventChannel;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<EventPublisherBackgroundService> _logger;

    public EventPublisherBackgroundService(IEventStore eventStore,
        IEventChannel eventChannel,
        IEventPublisher eventPublisher,
        ILogger<EventPublisherBackgroundService> logger,
        IOptionsMonitor<EventOptions> options,
        ResiliencePipelineProvider<string> resiliencePipelineProvider)
    {
        _eventStore = eventStore;
        _eventChannel = eventChannel;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _tasks = new Task[options.CurrentValue.MaxParallelWorkers];

        _receiverPipeline = resiliencePipelineProvider.GetPipeline(EventConstants.ReceiverPipelineName);
        _lockReleasePipeline = resiliencePipelineProvider.GetPipeline(EventConstants.EventLockReleasePipelineName);
        _publicationPipeline = resiliencePipelineProvider.GetPipeline(EventConstants.EventPublicationPipelineName);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var i = 0; i < _tasks.Length; i++)
        {
            _tasks[i] = Task.Factory.StartNew(async () =>
            {
                await _receiverPipeline.ExecuteAsync(RunAsync, stoppingToken);
            }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        return Task.WhenAll(_tasks);
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
                Delay = TimeSpan.FromSeconds(1), BackoffType = DelayBackoffType.Exponential, MaxRetryAttempts = 3
            })
            .Build();

    private static ResiliencePipeline CreatePublicationPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(1), BackoffType = DelayBackoffType.Exponential, MaxRetryAttempts = 3
            })
            .Build();

    private async ValueTask RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var eventInfo in _eventChannel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await PublishEventAsync(eventInfo, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while publishing events");
                    await AddFailedPublicationAttemptAsync(eventInfo, cancellationToken);
                }

                _logger.AwaitingNextEvent();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while publishing events");
            throw;
        }
    }

    private async Task AddFailedPublicationAttemptAsync(EventInfo eventInfo, CancellationToken cancellationToken) =>
        await _lockReleasePipeline.ExecuteAsync(
            async (ev, cts) => await _eventStore.AddFailedAttemptAsync(ev, cts),
            eventInfo, cancellationToken);

    private async Task<bool> PublishEventAsync(EventInfo eventInfo, CancellationToken cancellationToken)
    {
        var (wasLockAcquired, eventLog) =
            await _eventStore.TryAcquireLockAsync(eventInfo, cancellationToken);

        if (!wasLockAcquired)
        {
            _logger.EventLogAlreadyTaken(eventInfo.EventLogId);
            return false;
        }

        // Try to publish up to three times before releasing the lock
        await _publicationPipeline.ExecuteAsync(async (el, cts) =>
            await _eventPublisher.PublishAsync(el, cts), eventLog!, cancellationToken);

        // Try to mark as published up to ten times before going for re-publish
        await _lockReleasePipeline.ExecuteAsync(async (ev, cts) =>
            await _eventStore.MarkAsPublishedAsync(ev, cts), eventInfo, cancellationToken);

        return true;
    }
}
