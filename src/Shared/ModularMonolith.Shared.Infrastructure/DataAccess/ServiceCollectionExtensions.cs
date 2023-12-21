using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Abstract;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Internal;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public static class ServiceCollectionExtensions
{
    public static DataAccessBuilder AddDataAccess(this IServiceCollection serviceCollection,
        Action<DatabaseOptions> action)
    {
        var options = new DatabaseOptions();
        action(options);

        serviceCollection.AddOptions<DatabaseOptions>()
            .PostConfigure(action);

        serviceCollection.AddSingleton<DbConnectionFactory>()
            .AddSingleton<ITransactionalScopeFactory, TransactionalScopeFactory>()
            .AddDbContext<InternalDbContext>(c =>
            {
                c.UseNpgsql(options.ConnectionString);
                c.UseSnakeCaseNamingConvention();
            }).AddScoped<IEventLogDbContext>(sp => sp.GetRequiredService<InternalDbContext>());

        return new DataAccessBuilder(serviceCollection, options);
    }
}
