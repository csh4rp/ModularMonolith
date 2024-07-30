using Asp.Versioning;
using FluentValidation;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastrucutre.Messaging;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.RestApi.Authorization;
using ModularMonolith.Shared.RestApi.Middlewares;
using ModularMonolith.Shared.RestApi.Swagger;
using ModularMonolith.Shared.RestApi.Telemetry;
using ModularMonolith.Shared.Tracing;

namespace ModularMonolith.Shared.RestApi;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder RegisterModules(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Database")
                               ?? throw new ArgumentException("'ConnectionStrings:Database' is not configured");

        var modules = builder.Configuration.GetEnabledModules().ToList();

        foreach (var module in modules)
        {
            builder.Services.AddSingleton<AppModule>(_ => (AppModule)Activator.CreateInstance(module.GetType())!);
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
        //
        builder.Services
            .AddDataAccess(builder.Configuration, assemblies)
            .AddAuth(builder.Configuration)
            .AddTelemetryWithTracing(builder.Configuration, builder.Environment)
            .AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true)
            //     .AddPostgresMessaging<BaseDbContext>(connectionString!, assemblies)
            .AddMediator(assemblies)
            .AddMessaging(builder.Configuration, assemblies)
            // .AddEvents(assemblies)
            //     .AddAuditLogs()
            .AddIdentityContextAccessor()
            .AddHttpContextAccessor()
            //     .AddExceptionHandlers()
            //     .AddScoped<DbContext>(sp => sp.GetRequiredService<BaseDbContext>())
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
