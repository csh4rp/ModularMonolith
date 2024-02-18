using Asp.Versioning;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Bootstrapper.Infrastructure;
using ModularMonolith.Shared.Api.Authorization;
using ModularMonolith.Shared.Api.Exceptions;
using ModularMonolith.Shared.Api.Middlewares;
using ModularMonolith.Shared.Api.Swagger;
using ModularMonolith.Shared.Api.Telemetry;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Infrastructure.AuditLogs;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events;
using ModularMonolith.Shared.Infrastructure.Identity;
using ModularMonolith.Shared.Infrastructure.Messaging;
using ModularMonolith.Shared.Infrastructure.Messaging.Interceptors;

namespace ModularMonolith.Shared.Api;

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

        builder.Services
            .AddDbContextFactory<ApplicationDbContext>((sp, optionsBuilder) =>
            {
                optionsBuilder.UseNpgsql(connectionString);
                optionsBuilder.UseSnakeCaseNamingConvention();
                optionsBuilder.AddInterceptors(new AuditLogInterceptor(), new PublishEventsInterceptor());
                optionsBuilder.UseApplicationServiceProvider(sp);
            }, ServiceLifetime.Scoped)
            .AddDataAccess(c =>
            {
                c.ConnectionString = connectionString!;
            })
            .AddAuth(builder.Configuration)
            .AddTelemetryWithTracing(builder.Configuration, builder.Environment)
            .AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true)
            .AddPostgresMessaging<ApplicationDbContext>(connectionString!, assemblies)
            .AddMediator(assemblies)
            .AddEvents(assemblies)
            .AddAuditLogs()
            .AddIdentityServices()
            .AddHttpContextAccessor()
            .AddExceptionHandlers()
            .AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>())
            .AddSingleton(TimeProvider.System)
            .AddScoped<IdentityMiddleware>();

        foreach (var module in modules)
        {
            module.RegisterServices(builder.Services);
        }

        return builder;
    }
}
