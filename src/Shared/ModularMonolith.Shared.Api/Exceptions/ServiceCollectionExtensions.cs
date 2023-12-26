namespace ModularMonolith.Shared.Api.Exceptions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<ConflictExceptionHandler>()
            .AddExceptionHandler<CatchAllExceptionHandler>();

        return serviceCollection;
    }
}
