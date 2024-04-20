using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.DataAccess.Mongo.Transactions;
using ModularMonolith.Shared.Events.Mongo;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDataAccess(this IServiceCollection serviceCollection,
        string connectionString,
        string databaseName)
    {
        serviceCollection.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>()
            .AddSingleton<IMongoClient>(_ => new MongoClient(connectionString))
            .AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

        return serviceCollection;
    }

    public static IServiceCollection AddMongoOutbox(this IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<OutboxBackgroundService>();
}
