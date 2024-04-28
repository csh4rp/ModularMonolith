using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.DataAccess.EntityFramework.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.Transactions;

namespace ModularMonolith.Shared.DataAccess.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkDataAccess(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<DbConnectionFactory>();

        return serviceCollection;
    }
}
