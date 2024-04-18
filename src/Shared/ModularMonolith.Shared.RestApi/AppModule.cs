using System.Collections.Frozen;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ModularMonolith.Shared.RestApi;

public abstract class AppModule
{
    public virtual string Name => GetType().Name;

    public abstract IServiceCollection RegisterServices(IServiceCollection serviceCollection);

    public abstract WebApplication RegisterEndpoints(WebApplication app);

    public abstract FrozenSet<Assembly> Assemblies { get; }

    public virtual void SwaggerGenAction(SwaggerGenOptions options)
    {
    }

    public virtual void SwaggerUIAction(SwaggerUIOptions options)
    {
    }
}
