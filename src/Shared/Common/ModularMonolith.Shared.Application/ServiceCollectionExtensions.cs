using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Middlewares;

namespace ModularMonolith.Shared.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection serviceCollection, Assembly[] assemblies)
    {
        serviceCollection.AddMediatR(c =>
        {
            c.AddOpenBehavior(typeof(TracingMiddleware<,>));
            c.AddOpenBehavior(typeof(TransactionalMiddleware<,>));
            c.RegisterServicesFromAssemblies(assemblies);
        });

        return serviceCollection;
    }
}
