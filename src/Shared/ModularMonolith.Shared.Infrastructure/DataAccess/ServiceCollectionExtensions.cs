using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

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

        serviceCollection.AddSingleton<DbConnectionFactory>()
            .AddSingleton<ITransactionalScopeFactory, TransactionalScopeFactory>()
            .AddDbContextFactory<SharedDbContext>((sp, b) =>
            {
                b.UseNpgsql(options.ConnectionString);
                b.UseSnakeCaseNamingConvention();
                b.UseApplicationServiceProvider(sp);
            }).AddScoped<IEventLogDbContext>(sp => sp.GetRequiredService<SharedDbContext>());

        return serviceCollection;
    }
}
