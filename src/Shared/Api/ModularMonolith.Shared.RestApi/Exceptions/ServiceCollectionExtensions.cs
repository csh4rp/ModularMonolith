namespace ModularMonolith.Shared.RestApi.Exceptions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<EntityNotFountExceptionHandler>()
            .AddExceptionHandler<ConflictExceptionHandler>()
            .AddExceptionHandler<CatchAllExceptionHandler>();

        return serviceCollection;
    }
}
