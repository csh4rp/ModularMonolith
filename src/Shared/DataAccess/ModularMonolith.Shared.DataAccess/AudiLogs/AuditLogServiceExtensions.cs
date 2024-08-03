namespace ModularMonolith.Shared.DataAccess.AudiLogs;

public static class AuditLogServiceExtensions
{
    public static IServiceCollection AddAuditLogServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddScoped<IAuditMetaDataProvider, AuditMetaDataProvider>();
    }
}
