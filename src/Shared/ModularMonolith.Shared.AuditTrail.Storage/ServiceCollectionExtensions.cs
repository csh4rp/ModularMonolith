using ModularMonolith.Shared.AuditTrail.Storage.Factories;

namespace ModularMonolith.Shared.AuditTrail.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogs(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<AuditLogFactory>();

        return serviceCollection;
    }
}
