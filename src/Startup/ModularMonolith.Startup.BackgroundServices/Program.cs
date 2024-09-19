using FluentValidation;
using ModularMonolith.Infrastructure.DataAccess;
using ModularMonolith.Infrastructure.Messaging;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.Infrastructure;
using ModularMonolith.Shared.Tracing;
using ModularMonolith.Startup.BackgroundServices;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

var modules = builder.Configuration.GetEnabledModules().ToList();

foreach (var module in modules)
{
    builder.Services.AddSingleton<IAppModule>(_ => module);
}

var assemblies = modules.SelectMany(m => m.Assemblies).ToArray();

builder.Services
    .AddDataAccess(builder.Configuration, assemblies)
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
    .AddSingleton(TimeProvider.System)
    .AddTracingServices()
    .AddHostedService<CronBackgroundService>();

foreach (var module in modules)
{
    module.RegisterServices(builder.Services);
}

var app = builder.Build();

await app.RunAsync();

var host = builder.Build();
host.Run();
