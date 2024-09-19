using System.Collections.Frozen;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Infrastructure;

namespace ModularMonolith.CategoryManagement.Infrastructure;

public class CategoryManagementModule : IAppModule
{
    private const string RootNamespace = "ModularMonolith.CategoryManagement";

    private static readonly Assembly BusinessLogicAssembly = Assembly.Load($"{RootNamespace}.Application");
    private static readonly Assembly ContractsAssembly = Assembly.Load($"{RootNamespace}.Contracts");
    private static readonly Assembly DomainAssembly = Assembly.Load($"{RootNamespace}.Domain");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load($"{RootNamespace}.Infrastructure");

    public string Name => "CategoryManagementModule";

    public FrozenSet<Assembly> Assemblies { get; } = new[]
    {
        BusinessLogicAssembly, ContractsAssembly, DomainAssembly, InfrastructureAssembly
    }.ToFrozenSet();

    public IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddInfrastructure();

        return serviceCollection;
    }
}
