using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Mapping;
using ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Models;
using ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Options;
using ModularMonolith.Shared.DataAccess.Mongo.Outbox.BackgroundServices;
using ModularMonolith.Shared.DataAccess.Mongo.Transactions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDataAccess(this IServiceCollection serviceCollection,
        string databaseName)
    {
        serviceCollection
            .AddScoped<IUnitOfWork, UnitOfWork>()
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

        var options = new AuditTrailOptions();
        // optionsAction(options);

        var classMap = new EntityClassMap<AuditLogEntity>();
        classMap.MapIdMember(b => b.Id);
        classMap.MapProperty(b => b.EntityPropertyChanges).SetElementName("entity_property_changes");
        classMap.MapProperty(b => b.EntityKeys).SetElementName("entity_keys");
        classMap.MapProperty(b => b.EntityState).SetElementName("entity_state");
        classMap.MapProperty(b => b.EntityType).SetElementName("entity_type");
        classMap.MapProperty(b => b.MetaData).SetElementName("meta_data");
        classMap.MapProperty(b => b.Timestamp).SetElementName("timestamp");
        classMap.SetCollectionName(options.CollectionName);

        BsonClassMap.RegisterClassMap(classMap);

        return serviceCollection;
    }

    public static IServiceCollection AddMongoOutbox(this IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<OutboxBackgroundService>();
}
