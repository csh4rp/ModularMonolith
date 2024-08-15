namespace ModularMonolith.Shared.Tracing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTracingServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddScoped<IOperationContextAccessor, OperationContextAccessor>();
    }
}
