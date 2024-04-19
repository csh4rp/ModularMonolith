using ModularMonolith.Shared.Application.Abstract;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Transactions;

internal sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IMongoClient _mongoClient;

    public UnitOfWorkFactory(IMongoClient mongoClient) => _mongoClient = mongoClient;

    public async ValueTask<IUnitOfWork> CreateAsync(CancellationToken cancellationToken)
    {
        var session = await _mongoClient.StartSessionAsync(new ClientSessionOptions(), cancellationToken);
        return new UnitOfWork(session);
    }
}
