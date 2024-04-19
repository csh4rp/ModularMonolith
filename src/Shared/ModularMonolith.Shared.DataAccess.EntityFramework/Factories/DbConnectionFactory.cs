using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Factories;

public sealed class DbConnectionFactory
{
    private readonly DbContext _dbContext;

    public DbConnectionFactory(DbContext dbContext) => _dbContext = dbContext;

    public DbConnection Create() => _dbContext.Database.GetDbConnection();
}
