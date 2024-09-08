using System.Collections.Frozen;
using System.Reflection;
using Microsoft.OpenApi.Models;
using ModularMonolith.CategoryManagement.RestApi.Categories;
using ModularMonolith.CategoryManagement.Infrastructure;
using ModularMonolith.Shared.RestApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ModularMonolith.CategoryManagement.RestApi;

public sealed class CategoryManagementModule : AppModule
{
    private const string RootNamespace = "ModularMonolith.CategoryManagement";

    private static readonly Assembly BusinessLogicAssembly = Assembly.Load($"{RootNamespace}.Application");
    private static readonly Assembly ContractsAssembly = Assembly.Load($"{RootNamespace}.Contracts");
    private static readonly Assembly DomainAssembly = Assembly.Load($"{RootNamespace}.Domain");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load($"{RootNamespace}.Infrastructure");
    private static readonly Assembly MessagingAssembly = Assembly.Load($"{RootNamespace}.Messaging");

    public override WebApplication RegisterEndpoints(WebApplication app)
    {
        app.UseCategoryEndpoints();
        return app;
    }

    public override FrozenSet<Assembly> Assemblies { get; } = new[]
    {
        BusinessLogicAssembly, ContractsAssembly, DomainAssembly, InfrastructureAssembly, MessagingAssembly
    }.ToFrozenSet();

    public override IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddInfrastructure();

        return serviceCollection;
    }

    public override void SwaggerGenAction(SwaggerGenOptions options) =>
        options.SwaggerDoc("category-management-v1", new OpenApiInfo { Version = "v1.0" });

    public override void SwaggerUIAction(SwaggerUIOptions options) =>
        options.SwaggerEndpoint("/swagger/category-management-v1/swagger.json", "Category Management v1.0");
}
