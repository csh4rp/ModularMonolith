using FluentValidation;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastructure.Messaging;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.RestApi;
using ModularMonolith.Shared.RestApi.Authorization;
using ModularMonolith.Shared.RestApi.Exceptions;
using ModularMonolith.Shared.RestApi.Telemetry;
using ModularMonolith.Shared.Tracing;

var builder = Host.CreateApplicationBuilder(args);

var modules = builder.Configuration.GetEnabledModules().ToList();

foreach (var module in modules)
{
    builder.Services.AddSingleton<AppModule>(_ => (AppModule)Activator.CreateInstance(module.GetType())!);
}

var assemblies = modules.SelectMany(m => m.Assemblies).ToArray();

builder.Services
    .AddDataAccess(builder.Configuration, assemblies)
    .AddAuth(builder.Configuration)
    .AddTelemetryWithTracing(builder.Configuration, builder.Environment)
    .AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true)
    .AddMediator(assemblies)
    .AddMessaging(builder.Configuration, assemblies)
    .AddIdentityContextAccessor()
    .AddHttpContextAccessor()
    .AddExceptionHandlers()
    .AddSingleton(TimeProvider.System)
    .AddTracingServices();

foreach (var module in modules)
{
    module.RegisterServices(builder.Services);
}

var app = builder.Build();

await app.RunAsync();

var host = builder.Build();
host.Run();
