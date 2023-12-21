using System.Collections.Frozen;
using System.Reflection;

namespace ModularMonolith.Shared.Api;

public abstract class AppModule
{
    public virtual string Name => GetType().Name;

    public abstract IServiceCollection RegisterServices(IServiceCollection serviceCollection);

    public abstract WebApplication RegisterEndpoints(WebApplication app);

    public abstract FrozenSet<Assembly> Assemblies { get; }
}
