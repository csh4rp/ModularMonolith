using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.CategoryManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }
}
