using ModularMonolith.Shared.Application.Abstract;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Transactions;

internal sealed class UnitOfWork : IUnitOfWork
{
    private static readonly ClientSessionOptions DefaultOptions = new() { CausalConsistency = true };

    private readonly IMongoClient _mongoClient;

    public UnitOfWork(IMongoClient mongoClient) => _mongoClient = mongoClient;

    public async Task<IUnitOfWorkScope> BeginScopeAsync(CancellationToken cancellationToken)
    {
        var session = await _mongoClient.StartSessionAsync(DefaultOptions, cancellationToken);
        return new UnitOfWorkScope(session);
    }
}
