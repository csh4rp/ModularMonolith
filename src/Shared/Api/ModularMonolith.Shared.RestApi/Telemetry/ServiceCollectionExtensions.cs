﻿using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularMonolith.Shared.RestApi.Telemetry;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryWithTracing(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        serviceCollection.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder.AddSource("ModularMonolith")
                    .ConfigureResource(resource =>
                        resource.AddService(
                            serviceName: "ModularMonolith",
                            serviceVersion: "1.0.0"))
                    .AddAspNetCoreInstrumentation();

                if (environment.IsDevelopment())
                {
                    builder.AddConsoleExporter();
                }
            });

        return serviceCollection;
    }
}
