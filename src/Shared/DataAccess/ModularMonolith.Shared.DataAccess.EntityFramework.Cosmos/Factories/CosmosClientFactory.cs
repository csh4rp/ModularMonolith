using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.Factories;

public class CosmosClientFactory
{
    private readonly DbContext _dbContext;

    public CosmosClientFactory(DbContext dbContext) => _dbContext = dbContext;

    public CosmosClient Create() => _dbContext.Database.GetCosmosClient();
}
