using ModularMonolith.Shared.AuditTrail.EntityFramework.Factories;

namespace ModularMonolith.Shared.AuditTrail.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogs(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<AuditLogFactory>();

        return serviceCollection;
    }
}
