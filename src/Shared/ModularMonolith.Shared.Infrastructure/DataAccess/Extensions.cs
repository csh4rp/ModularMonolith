using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public static class Extensions
{
    public static IServiceCollection AddDb<TDbContext>(this IServiceCollection serviceCollection,
        IConfiguration configuration) where TDbContext : BaseDbContext
    {
        serviceCollection.AddDbContext<TDbContext>(c =>
        {
            c.UseSnakeCaseNamingConvention();
        });
        
        return serviceCollection;
    }
}
