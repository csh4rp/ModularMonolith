using FluentValidation;
using ModularMonolith.Shared.Api.Exceptions;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events;
using ModularMonolith.Shared.Infrastructure.Identity;

namespace ModularMonolith.Shared.Api;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder RegisterModules(this WebApplicationBuilder builder)
    {
        var modules = builder.Configuration.GetEnabledModules().ToList();

        foreach (var module in modules)
        {
            builder.Services.AddSingleton<AppModule>(sp => (AppModule) Activator.CreateInstance(module.GetType())!);
        }
        
        var assemblies = modules.SelectMany(m => m.Assemblies).ToArray();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddMediator(assemblies);
        builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        builder.Services.AddEvents(e =>
        {
            e.Assemblies = [.. assemblies];
        });

        builder.Services
            .AddSingleton(TimeProvider.System)
            .AddIdentityServices()
            .AddHttpContextAccessor()
            .AddExceptionHandlers();

        builder.Services.AddDataAccess(c =>
        {
            c.ConnectionString = builder.Configuration.GetConnectionString("Database")!;
        });

        foreach (var module in modules)
        {
            module.RegisterServices(builder.Services);
        }
        
        return builder;
    }
}
