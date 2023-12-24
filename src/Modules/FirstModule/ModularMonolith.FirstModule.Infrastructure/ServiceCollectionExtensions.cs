using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.FirstModule.Application.Categories.Abstract;
using ModularMonolith.FirstModule.Infrastructure.Categories.DataAccess;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;

[assembly: InternalsVisibleTo("ModularMonolith.FirstModule.Application.UnitTests")]
[assembly: InternalsVisibleTo("ModularMonolith.FirstModule.Migrations")]

namespace ModularMonolith.FirstModule.Infrastructure;

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
