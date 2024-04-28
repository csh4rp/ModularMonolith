using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.DataAccess.Mongo.Transactions;
using ModularMonolith.Shared.Events.Mongo;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDataAccess(this IServiceCollection serviceCollection,
        string databaseName)
    {
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>()
            .AddSingleton<IMongoClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("Database");
                return new MongoClient(connectionString);
            })
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
