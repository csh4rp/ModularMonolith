using System.Diagnostics;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.Extensions;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using Polly;
using Polly.Retry;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;

internal sealed class EventPublisherBackgroundService : BackgroundService
{
    private static readonly ActivitySource EventPublisherActivitySource = new ActivitySource("");
    
    private readonly EventReader _eventReader;
    private readonly EventSerializer _eventSerializer;
    private readonly EventMapper _eventMapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EventPublisherBackgroundService> _logger;


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
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 5
            })
            .Build();
        
        try
        {
            await foreach (var eventLog in _eventReader.GetUnpublishedEventsAsync(cancellationToken))
            {
                await pipeline.ExecuteAsync(PublishAsync, eventLog, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while fetching unpublished events");
        }
    }

    private async ValueTask PublishAsync(EventLog eventLog, CancellationToken cancellationToken)
    {
        var @event = _eventSerializer.Deserialize(eventLog.Type, eventLog.Payload);

        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            using var activity =
                EventPublisherActivitySource.CreateActivity(eventLog.Name, ActivityKind.Internal);

            activity?.SetParentId(eventLog.ActivityId);
            activity?.Start();
            
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Publish(@event, cancellationToken);
        }
        
        _logger.EventPublished(eventLog.Id);

        if (_eventMapper.TryMap(@event, out var integrationEvent))
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                using var activity =
                    EventPublisherActivitySource.CreateActivity(eventLog.Name, ActivityKind.Internal);

                activity?.SetParentId(eventLog.ActivityId);
                activity?.Start();
            
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Publish(integrationEvent, cancellationToken);
            }
            

            _logger.IntegrationEventPublished(eventLog.Id);
        }
    }
}
