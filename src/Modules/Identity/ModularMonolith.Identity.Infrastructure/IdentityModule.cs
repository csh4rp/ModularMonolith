using System.Collections.Frozen;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Identity.Core.Options;
using ModularMonolith.Shared.Infrastructure;

namespace ModularMonolith.Identity.Infrastructure;

public class IdentityModule : IAppModule
{
    private const string RootNamespace = "ModularMonolith.Identity";

    private static readonly Assembly BusinessLogicAssembly = Assembly.Load($"{RootNamespace}.Application");
    private static readonly Assembly ContractsAssembly = Assembly.Load($"{RootNamespace}.Contracts");
    private static readonly Assembly DomainAssembly = Assembly.Load($"{RootNamespace}.Domain");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load($"{RootNamespace}.Infrastructure");

    public string Name => "IdentityModule";

    public IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<AuthOptions>()
            .Configure<IConfiguration>((opt, conf) =>
            {
                conf.GetSection("Modules:Identity:Auth").Bind(opt);
            });

        serviceCollection.AddInfrastructure();
        return serviceCollection;
    }

    public FrozenSet<Assembly> Assemblies { get; } = new[]
    {
        BusinessLogicAssembly, ContractsAssembly, DomainAssembly, InfrastructureAssembly
    }.ToFrozenSet();
}
