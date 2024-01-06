namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventUtils(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton<IEventChannel, EventChannel>()
            .AddSingleton<IEventPublisher, EventPublisher>()
            .AddSingleton<EventSerializer>();
}
