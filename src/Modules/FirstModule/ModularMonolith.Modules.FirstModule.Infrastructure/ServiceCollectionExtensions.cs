using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.Abstract;
using ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess.Categories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;

[assembly: InternalsVisibleTo("ModularMonolith.Modules.FirstModule.BusinessLogic.Tests.Unit")]

namespace ModularMonolith.Modules.FirstModule.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContextFactory<CategoryDbContext>((sp, opt) =>
        {
            var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();

            opt.UseNpgsql(options.Value.ConnectionString);
            opt.UseSnakeCaseNamingConvention();
        });

        serviceCollection.AddScoped<ICategoryDatabase>(sp => sp.GetRequiredService<CategoryDbContext>());
        
        return serviceCollection;
    }
}
