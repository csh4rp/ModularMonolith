using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;

[assembly: InternalsVisibleTo("ModularMonolith.CategoryManagement.Application.UnitTests")]
[assembly: InternalsVisibleTo("ModularMonolith.CategoryManagement.Migrations")]

namespace ModularMonolith.CategoryManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContextFactory<CategoryManagementDbContext>((sp, opt) =>
        {
            var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();

            opt.UseNpgsql(options.Value.ConnectionString);
            opt.UseSnakeCaseNamingConvention();
            opt.UseApplicationServiceProvider(sp);
        });

        serviceCollection.AddScoped<ICategoryDatabase>(sp => sp.GetRequiredService<CategoryManagementDbContext>());

        return serviceCollection;
    }
}
