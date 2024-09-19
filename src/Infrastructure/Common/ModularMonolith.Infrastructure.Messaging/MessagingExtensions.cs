using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.Messaging.Kafka;
using ModularMonolith.Infrastructure.Messaging.Postgres;
using ModularMonolith.Infrastructure.Messaging.RabbitMQ;
using ModularMonolith.Infrastructure.Messaging.SqlServer;

namespace ModularMonolith.Infrastructure.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] assemblies)
    {
        var messagingProvider = configuration.GetSection("Messaging")
            .GetValue<string>("Provider");

        var runConsumers = configuration.GetSection("Messaging")
            .GetValue<bool>("RunConsumers");

        switch (messagingProvider)
        {
            case "Postgres":
                serviceCollection.AddPostgresMessaging<DbContext>(configuration, assemblies, runConsumers);
                break;
            case "SqlServer":
                serviceCollection.AddSqlServerMessaging<DbContext>(configuration, assemblies, runConsumers);
                break;
            case "Kafka":
                serviceCollection.AddKafkaMessaging<DbContext>(configuration, assemblies, runConsumers);
                break;
            case "RabbitMQ":
                serviceCollection.AddRabbitMQMessaging<DbContext>(configuration, assemblies, runConsumers);
                break;
            default:
                throw new ArgumentException("Messaging:Provider is required");
        }

        return serviceCollection;
    }
}
