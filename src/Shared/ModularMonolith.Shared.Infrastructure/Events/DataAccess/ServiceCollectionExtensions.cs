using ModularMonolith.Shared.Application.Events;
using Polly;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;
using Polly.Retry;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventDataAccessServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEventBus, OutboxEventBus>()
            .AddScoped<IEventLogStore, EventLogStore>()
            .AddSingleton<IEventStore, EventStore>();

        serviceCollection.AddResiliencePipeline(EventConstants.ReceiverPipelineName, builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions
                {
                    Delay = TimeSpan.FromSeconds(5),
                    BackoffType = DelayBackoffType.Constant,
                    MaxRetryAttempts = int.MaxValue
                });
        });
        
        serviceCollection.AddResiliencePipeline(EventConstants.EventLockReleasePipelineName, builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions
                {
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 3
                });
        });
        
        serviceCollection.AddResiliencePipeline(EventConstants.EventPublicationPipelineName, builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions
                {
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 3
                });
        });
        
        return serviceCollection;
    }
}
