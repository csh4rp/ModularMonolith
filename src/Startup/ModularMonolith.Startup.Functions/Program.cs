using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastructure.Messaging;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.RestApi;
using ModularMonolith.Shared.RestApi.Exceptions;
using ModularMonolith.Shared.RestApi.Telemetry;
using ModularMonolith.Shared.Tracing;

var builder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults();

builder.ConfigureServices((context, serviceCollection) =>
{
    var modules = context.Configuration.GetEnabledModules().ToList();
    var assemblies = modules.SelectMany(m => m.Assemblies).ToArray();

    foreach (var module in modules)
    {
        serviceCollection.AddSingleton<AppModule>(_ => (AppModule)Activator.CreateInstance(module.GetType())!);
    }

    serviceCollection.AddDataAccess(context.Configuration, assemblies)
        .AddTelemetryWithTracing(context.Configuration, context.HostingEnvironment)
        .AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true)
        .AddMediator(assemblies)
        .AddMessaging(context.Configuration, assemblies)
        .AddIdentityContextAccessor()
        .AddHttpContextAccessor()
        .AddExceptionHandlers()
        .AddSingleton(TimeProvider.System)
        .AddTracingServices();
});

var host = builder.Build();


host.Run();
