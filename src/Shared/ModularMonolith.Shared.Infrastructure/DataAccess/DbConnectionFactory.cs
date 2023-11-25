using Npgsql;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public class DbConnectionFactory
{
    public NpgsqlConnection Create()
    {
        return new NpgsqlConnection("");
    }
}
