using Asp.Versioning;
using FluentValidation;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastructure.Messaging;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.RestApi.Authorization;
using ModularMonolith.Shared.RestApi.Exceptions;
using ModularMonolith.Shared.RestApi.Middlewares;
using ModularMonolith.Shared.RestApi.Swagger;
using ModularMonolith.Shared.Tracing;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularMonolith.Shared.RestApi;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder RegisterModules(this WebApplicationBuilder builder)
    {
        var modules = builder.Configuration.GetEnabledWebModules().ToList();

        foreach (var module in modules)
        {
            builder.Services.AddSingleton<IWebAppModule>(
                _ => (IWebAppModule)Activator.CreateInstance(module.GetType())!);
        }

        var assemblies = modules.SelectMany(m => m.Assemblies).ToArray();

        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new MediaTypeApiVersionReader();
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSwaggerWithBearerToken(modules);
        }

        builder.Services
            .AddDataAccess(builder.Configuration, assemblies)
            .AddAuth(builder.Configuration)
            .AddOpenTelemetry()
            .WithTracing(b =>
            {
                b.AddSource("ModularMonolith")
                    .ConfigureResource(resource =>
                        resource.AddService(
                            serviceName: "ModularMonolith",
                            serviceVersion: "1.0.0"));

                if (builder.Environment.IsDevelopment())
                {
                    b.AddConsoleExporter();
                }
            });

        builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true)
            .AddMediator(assemblies)
            .AddMessaging(builder.Configuration, assemblies)
            .AddIdentityContextAccessor()
            .AddHttpContextAccessor()
            .AddExceptionHandlers()
            .AddSingleton(TimeProvider.System)
            .AddTracingServices()
            .AddScoped<IdentityMiddleware>();

        foreach (var module in modules)
        {
            module.RegisterServices(builder.Services);
        }

        return builder;
    }
}
