namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventMetaDataProvider(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton<EventMetaDataProvider>();
}
