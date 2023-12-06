using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.BusinessLogic;

public static class Extensions
{
    public static IServiceCollection AddMediator(this IServiceCollection serviceCollection, Assembly[] assemblies)
    {
        serviceCollection.AddMediatR(c =>
        {
            c.RegisterServicesFromAssemblies(assemblies);
        });
        
        return serviceCollection;
    }
}
