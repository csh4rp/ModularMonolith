using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.DataAccess.Factories;
using ModularMonolith.Shared.DataAccess.Options;

namespace ModularMonolith.Shared.DataAccess.SqlServer.Factories;

internal sealed class SqlServerDbConnectionFactory : IDbConnectionFactory
{
    private readonly IOptionsMonitor<DatabaseOptions> _optionsMonitor;

    public SqlServerDbConnectionFactory(IOptionsMonitor<DatabaseOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public DbConnection Create() => new SqlConnection(_optionsMonitor.CurrentValue.ConnectionString);
}
