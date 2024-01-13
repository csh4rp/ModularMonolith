using ModularMonolith.Shared.Infrastructure.DataAccess.Options;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection serviceCollection,
        Action<DatabaseOptions> action)
    {
        var options = new DatabaseOptions();
        action(options);

        serviceCollection.AddOptions<DatabaseOptions>()
            .PostConfigure(action);

        return serviceCollection;
    }
}
