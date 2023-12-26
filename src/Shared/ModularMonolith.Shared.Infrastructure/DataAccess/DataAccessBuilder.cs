using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public class DataAccessBuilder
{
    private readonly IServiceCollection _serviceCollection;
    private readonly DatabaseOptions _options;

    public DataAccessBuilder(IServiceCollection serviceCollection, DatabaseOptions options)
    {
        _serviceCollection = serviceCollection;
        _options = options;
    }

    public DataAccessBuilder AddDbContext<TDbContext>() where TDbContext : BaseDbContext
    {
        _serviceCollection.AddDbContext<TDbContext>(o =>
        {
            o.AddInterceptors(new AuditLogInterceptor());
            o.UseNpgsql(_options.ConnectionString);
            o.UseSnakeCaseNamingConvention();
        });

        return this;
    }
}
