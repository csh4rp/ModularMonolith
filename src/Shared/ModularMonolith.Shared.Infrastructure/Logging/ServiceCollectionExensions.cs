using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ModularMonolith.Shared.Infrastructure.Logging;

public static class ServiceCollectionExensions
{
    public static IServiceCollection AddLogging(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        return serviceCollection;
    }
}
