using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Abstract;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess<TDbContext>(this IServiceCollection serviceCollection,
        Action<DatabaseOptions> action) where TDbContext : BaseDbContext
    {
        var options = new DatabaseOptions();
        action(options);
        
        serviceCollection.AddOptions<DatabaseOptions>()
            .PostConfigure(action);
        
        serviceCollection.AddScoped<DbConnectionFactory>()
            .AddSingleton<ITransactionalScopeFactory, TransactionalScopeFactory>()
            .AddDbContext<TDbContext>(c =>
        {
            c.UseNpgsql(options.ConnectionString);
            c.UseSnakeCaseNamingConvention();
        });
        
        return serviceCollection;
    }
}
