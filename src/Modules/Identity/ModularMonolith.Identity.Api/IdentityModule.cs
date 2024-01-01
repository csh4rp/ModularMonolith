using System.Collections.Frozen;
using System.Reflection;
using Microsoft.OpenApi.Models;
using ModularMonolith.Identity.Api.Account;
using ModularMonolith.Identity.Core.Options;
using ModularMonolith.Identity.Infrastructure;
using ModularMonolith.Shared.Api;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ModularMonolith.Identity.Api;

public sealed class IdentityModule : AppModule
{
    private const string RootNamespace = "ModularMonolith.Identity";

    private static readonly Assembly BusinessLogicAssembly = Assembly.Load($"{RootNamespace}.Application");
    private static readonly Assembly ContractsAssembly = Assembly.Load($"{RootNamespace}.Contracts");
    private static readonly Assembly DomainAssembly = Assembly.Load($"{RootNamespace}.Domain");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load($"{RootNamespace}.Infrastructure");

    public override IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<AuthOptions>()
            .Configure<IConfiguration>((opt, conf) =>
        {
            conf.GetSection("Modules:Identity:Auth").Bind(opt);
        });
        
        serviceCollection.AddInfrastructure();
        return serviceCollection;
    }

    public override WebApplication RegisterEndpoints(WebApplication app)
    {
        app.UseAccountEndpoints();
        return app;
    }

    public override FrozenSet<Assembly> Assemblies { get; } = new[]
    {
        BusinessLogicAssembly, ContractsAssembly, DomainAssembly, InfrastructureAssembly
    }.ToFrozenSet();
    
    public override void SwaggerGenAction(SwaggerGenOptions options)
    {
        options.SwaggerDoc("identity-v1", new OpenApiInfo
        {
            Version = "v1.0"
        });
    }

    public override void SwaggerUIAction(SwaggerUIOptions options)
    {
        options.SwaggerEndpoint("/swagger/identity-v1/swagger.json", "Identity v1.0");
    }
}
