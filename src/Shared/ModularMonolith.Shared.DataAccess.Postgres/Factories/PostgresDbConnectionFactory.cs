using System.Data.Common;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.DataAccess.Factories;
using ModularMonolith.Shared.DataAccess.Options;
using Npgsql;

namespace ModularMonolith.Shared.DataAccess.Postgres.Factories;

internal sealed class PostgresDbConnectionFactory : IDbConnectionFactory
{
    private readonly IOptionsMonitor<DatabaseOptions> _optionsMonitor;

    public PostgresDbConnectionFactory(IOptionsMonitor<DatabaseOptions> optionsMonitor) => _optionsMonitor = optionsMonitor;

    public DbConnection Create() => new NpgsqlConnection(_optionsMonitor.CurrentValue.ConnectionString);
}
