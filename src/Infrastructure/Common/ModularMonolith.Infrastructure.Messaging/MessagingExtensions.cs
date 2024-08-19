using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.Messaging.Kafka;
using ModularMonolith.Infrastructure.Messaging.Postgres;
using ModularMonolith.Infrastructure.Messaging.RabbitMQ;

namespace ModularMonolith.Infrastructure.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] assemblies)
    {
        var messagingProvider = configuration.GetSection("Messaging")
            .GetSection("Provider")
            .Value;

        switch (messagingProvider)
        {
            case "Postgres":
                serviceCollection.AddPostgresMessaging<DbContext>(configuration, assemblies);
                break;
            case "Kafka":
                serviceCollection.AddKafkaMessaging<DbContext>(configuration, assemblies);
                break;
            case "RabbitMQ":
                serviceCollection.AddRabbitMQMessaging<DbContext>(configuration, assemblies);
                break;
            default:
                throw new ArgumentException("Messaging:Provider is required");
        }

        return serviceCollection;
    }
}
