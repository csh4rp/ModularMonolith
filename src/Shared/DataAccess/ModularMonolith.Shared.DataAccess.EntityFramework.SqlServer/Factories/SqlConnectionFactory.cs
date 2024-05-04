using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.Factories;

public class SqlConnectionFactory
{
    private readonly DbContext _dbContext;

    public SqlConnectionFactory(DbContext dbContext) => _dbContext = dbContext;

    public SqlConnection Create() => (SqlConnection)_dbContext.Database.GetDbConnection();
}
