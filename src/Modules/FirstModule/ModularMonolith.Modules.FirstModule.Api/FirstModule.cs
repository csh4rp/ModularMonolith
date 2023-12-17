using System.Reflection;
using ModularMonolith.Modules.FirstModule.Infrastructure;
using ModularMonolith.Shared.Api;

namespace ModularMonolith.Modules.FirstModule.Api;

public sealed class FirstModule : AppModule
{
    private const string RootNamespace = "ModularMonolith.Modules.FirstModule";

    private readonly Assembly _businessLogicAssembly = Assembly.Load($"{RootNamespace}.BusinessLogic");
    private readonly Assembly _contractsAssembly = Assembly.Load($"{RootNamespace}.Contracts");
    private readonly Assembly _domainAssembly = Assembly.Load($"{RootNamespace}.Domain");
    private readonly Assembly _infrastructureAssembly = Assembly.Load($"{RootNamespace}.Infrastructure");


    public override Assembly[] GetHandlersAssemblies() => new[] { _businessLogicAssembly, _infrastructureAssembly };
    
    public override Assembly[] GetValidatorsAssemblies() => new[] { _contractsAssembly };
    
    public override IServiceCollection RegisterServices(IServiceCollection serviceCollection) 
    {

        serviceCollection.AddInfrastructure();

        return serviceCollection;
    }
}
