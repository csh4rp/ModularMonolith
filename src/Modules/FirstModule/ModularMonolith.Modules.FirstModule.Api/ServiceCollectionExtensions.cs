namespace ModularMonolith.Modules.FirstModule.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFirstModule(this IServiceCollection serviceCollection)
    {
        var module = new FirstModule();

        module.RegisterServices(serviceCollection);

        return serviceCollection;
    }
}
