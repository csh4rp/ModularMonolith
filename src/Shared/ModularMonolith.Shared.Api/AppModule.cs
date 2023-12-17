using System.Reflection;

namespace ModularMonolith.Shared.Api;

public abstract class AppModule
{
    public virtual string Name => GetType().Name;

    public abstract IServiceCollection RegisterServices(IServiceCollection serviceCollection);

    public abstract Assembly[] GetHandlersAssemblies();
    
    public abstract Assembly[] GetValidatorsAssemblies();
}
