using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Factories;

public class PostgresConnectionFactory
{
    private readonly DbContext _dbContext;

    public PostgresConnectionFactory(DbContext dbContext) => _dbContext = dbContext;

    public NpgsqlConnection Create() => (NpgsqlConnection)_dbContext.Database.GetDbConnection();
}
