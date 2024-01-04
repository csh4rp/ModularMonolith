namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventUtils(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton<EventMapper>()
            .AddSingleton<EventChannel>()
            .AddSingleton<IEventPublisher, EventPublisher>()
            .AddSingleton<EventSerializer>();
}
