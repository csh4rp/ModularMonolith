using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Mapping;
using ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Models;
using ModularMonolith.Shared.DataAccess.Mongo.Options;
using ModularMonolith.Shared.DataAccess.Mongo.Outbox.BackgroundServices;
using ModularMonolith.Shared.DataAccess.Mongo.Transactions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDataAccess(this IServiceCollection serviceCollection,
        Action<MongoOptions> optionsSetup)
    {
        serviceCollection.AddOptionsWithValidateOnStart<MongoOptions>()
            .PostConfigure(optionsSetup);

        serviceCollection
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IMongoClient>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var options = serviceProvider.GetRequiredService<IOptions<MongoOptions>>().Value;
                var connectionString = configuration.GetConnectionString(options.ConnectionStringName);

                return new MongoClient(connectionString);
            })
            .AddScoped<IMongoDatabase>(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<IMongoClient>();
                var options = serviceProvider.GetRequiredService<IOptions<MongoOptions>>();
                return client.GetDatabase(options.Value.DatabaseName);
            });

        var options = new MongoOptions();
        optionsSetup(options);

        var classMap = new EntityClassMap<AuditLogEntity>();
        classMap.MapIdMember(b => b.Id);
        classMap.MapProperty(b => b.EntityPropertyChanges).SetElementName("entity_property_changes");
        classMap.MapProperty(b => b.EntityKeys).SetElementName("entity_keys");
        classMap.MapProperty(b => b.EntityState).SetElementName("entity_state");
        classMap.MapProperty(b => b.EntityType).SetElementName("entity_type");
        classMap.MapProperty(b => b.MetaData).SetElementName("meta_data");
        classMap.MapProperty(b => b.Timestamp).SetElementName("timestamp");
        classMap.SetCollectionName(options.AuditLogOptions.CollectionName);

        BsonClassMap.RegisterClassMap(classMap);

        return serviceCollection;
    }

    public static IServiceCollection AddMongoOutbox(this IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<OutboxBackgroundService>();
}
