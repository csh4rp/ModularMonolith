using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        Action<MongoOptions> optionsSetup,
        Action<AuditLogOptions> auditLogOptionsSetup)
    {
        serviceCollection.AddOptionsWithValidateOnStart<MongoOptions>()
            .PostConfigure(optionsSetup);

        serviceCollection.AddOptionsWithValidateOnStart<AuditLogOptions>()
            .PostConfigure(auditLogOptionsSetup);
        
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
                var options = sp.GetRequiredService<IOptions<MongoOptions>>();
                return client.GetDatabase(options.Value.DatabaseName);
            });


        var auditLogOptions = new AuditLogOptions();
        auditLogOptionsSetup(auditLogOptions);
        
        var classMap = new EntityClassMap<AuditLogEntity>();
        classMap.MapIdMember(b => b.Id);
        classMap.MapProperty(b => b.EntityPropertyChanges).SetElementName("entity_property_changes");
        classMap.MapProperty(b => b.EntityKeys).SetElementName("entity_keys");
        classMap.MapProperty(b => b.EntityState).SetElementName("entity_state");
        classMap.MapProperty(b => b.EntityType).SetElementName("entity_type");
        classMap.MapProperty(b => b.MetaData).SetElementName("meta_data");
        classMap.MapProperty(b => b.Timestamp).SetElementName("timestamp");
        classMap.SetCollectionName(auditLogOptions.CollectionName);

        BsonClassMap.RegisterClassMap(classMap);

        return serviceCollection;
    }

    public static IServiceCollection AddMongoOutbox(this IServiceCollection serviceCollection) =>
        serviceCollection.AddHostedService<OutboxBackgroundService>();
}
