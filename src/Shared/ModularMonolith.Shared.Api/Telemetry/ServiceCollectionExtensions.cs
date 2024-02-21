using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularMonolith.Shared.Api.Telemetry;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryWithTracing(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        serviceCollection.AddOpenTelemetry()
            .WithTracing(b =>
            {
                b.AddSource("ModularMonolith")
                    .ConfigureResource(resource =>
                        resource.AddService(
                            serviceName: "ModularMonolith",
                            serviceVersion: "1.0.0"))
                    .AddAspNetCoreInstrumentation();

                if (environment.IsDevelopment())
                {
                    b.AddConsoleExporter();
                }
            });

        return serviceCollection;
    }

}
