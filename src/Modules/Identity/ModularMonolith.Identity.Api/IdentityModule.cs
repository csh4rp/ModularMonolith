using System.Collections.Frozen;
using System.Reflection;
using ModularMonolith.Shared.Api;

namespace ModularMonolith.Identity.Api;

public sealed class IdentityModule : AppModule
{
    private const string RootNamespace = "ModularMonolith.Identity";

    private static readonly Assembly BusinessLogicAssembly = Assembly.Load($"{RootNamespace}.BusinessLogic");
    private static readonly Assembly ContractsAssembly = Assembly.Load($"{RootNamespace}.Contracts");
    private static readonly Assembly DomainAssembly = Assembly.Load($"{RootNamespace}.Domain");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load($"{RootNamespace}.Infrastructure");

    public override IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }

    public override WebApplication RegisterEndpoints(WebApplication app)
    {
        return app;
    }

    public override FrozenSet<Assembly> Assemblies { get; } = new[]
    {
        BusinessLogicAssembly, ContractsAssembly, DomainAssembly, InfrastructureAssembly
    }.ToFrozenSet();
}
