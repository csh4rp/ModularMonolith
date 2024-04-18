using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.DataAccess.Options;
using ModularMonolith.Shared.DataAccess.Transactions;

namespace ModularMonolith.Shared.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection serviceCollection,
        Action<DatabaseOptions> action)
    {
        var options = new DatabaseOptions();
        action(options);

        serviceCollection.AddOptions<DatabaseOptions>()
            .PostConfigure(action);

        serviceCollection.AddSingleton<ITransactionalScopeFactory, TransactionalScopeFactory>();

        return serviceCollection;
    }
}
