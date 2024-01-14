using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.Concrete;

namespace ModularMonolith.CategoryManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICategoryDatabase>(sp =>
        {
            var context = sp.GetRequiredService<DbContext>();
            return new CategoryDatabase(context);
        });

        return serviceCollection;
    }
}
