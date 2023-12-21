using System.Reflection;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Middlewares;

namespace ModularMonolith.Shared.BusinessLogic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection serviceCollection, Assembly[] assemblies)
    {
        serviceCollection.AddMediatR(c =>
        {
            c.NotificationPublisher = new ForeachAwaitPublisher();
            c.AddOpenBehavior(typeof(TracingMiddleware<,>));
            c.AddOpenBehavior(typeof(TransactionalMiddleware<,>));
            c.RegisterServicesFromAssemblies(assemblies);
        });
        
        return serviceCollection;
    }
}
