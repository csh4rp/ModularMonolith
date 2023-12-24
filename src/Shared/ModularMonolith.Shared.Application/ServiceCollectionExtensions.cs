using System.Reflection;
using System.Runtime.CompilerServices;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Middlewares;

[assembly: InternalsVisibleTo("ModularMonolith.FirstModule.Application.UnitTests")]

namespace ModularMonolith.Shared.Application;

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
