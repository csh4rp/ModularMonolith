using System.Collections.Frozen;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Infrastructure;

public interface IAppModule
{
    public string Name { get; }

    public IServiceCollection RegisterServices(IServiceCollection serviceCollection);

    public FrozenSet<Assembly> Assemblies { get; }
}
