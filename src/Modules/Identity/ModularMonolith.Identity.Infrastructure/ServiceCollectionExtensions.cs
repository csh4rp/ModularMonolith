using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Identity.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;

namespace ModularMonolith.Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContextFactory<IdentityDbContext>((sp, opt) =>
        {
            var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();

            opt.UseNpgsql(options.Value.ConnectionString);
            opt.UseSnakeCaseNamingConvention();
        });
        
        return serviceCollection;
    }
}
