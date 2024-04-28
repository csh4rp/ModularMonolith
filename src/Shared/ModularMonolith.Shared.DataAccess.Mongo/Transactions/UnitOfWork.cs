using ModularMonolith.Shared.Application.Abstract;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Transactions;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly IMongoClient _mongoClient;

    public UnitOfWork(IMongoClient mongoClient) => _mongoClient = mongoClient;

    public async ValueTask<IUnitOfWorkScope> CreateAsync(CancellationToken cancellationToken)
    {
        var session = await _mongoClient.StartSessionAsync(new ClientSessionOptions(), cancellationToken);
        return new UnitOfWorkScope(session);
    }
}
