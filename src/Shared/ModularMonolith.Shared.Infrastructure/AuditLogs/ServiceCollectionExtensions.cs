using ModularMonolith.Shared.Infrastructure.AuditLogs.Factories;

namespace ModularMonolith.Shared.Infrastructure.AuditLogs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogs(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<AuditLogFactory>();
        
        return serviceCollection;
    }
}
