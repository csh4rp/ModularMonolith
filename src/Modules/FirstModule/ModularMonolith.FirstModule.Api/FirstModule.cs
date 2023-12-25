﻿using System.Collections.Frozen;
using System.Reflection;
using ModularMonolith.FirstModule.Api.Categories;
using ModularMonolith.FirstModule.Infrastructure;
using ModularMonolith.Shared.Api;

namespace ModularMonolith.FirstModule.Api;

public sealed class FirstModule : AppModule
{
    private const string RootNamespace = "ModularMonolith.FirstModule";

    private static readonly Assembly BusinessLogicAssembly = Assembly.Load($"{RootNamespace}.Application");
    private static readonly Assembly ContractsAssembly = Assembly.Load($"{RootNamespace}.Contracts");
    private static readonly Assembly DomainAssembly = Assembly.Load($"{RootNamespace}.Domain");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load($"{RootNamespace}.Infrastructure");

    public override WebApplication RegisterEndpoints(WebApplication app)
    {
        app.UseCategoryEndpoints();

        return app;
    }

    public override FrozenSet<Assembly> Assemblies { get; } = new[]
    {
        BusinessLogicAssembly, ContractsAssembly, DomainAssembly, InfrastructureAssembly
    }.ToFrozenSet();

    public override IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddInfrastructure();

        return serviceCollection;
    }
}
