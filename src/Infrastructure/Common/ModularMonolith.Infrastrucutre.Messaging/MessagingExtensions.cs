using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging.MassTransit;
using ModularMonolith.Shared.Messaging.MassTransit.Kafka;
using ModularMonolith.Shared.Messaging.MassTransit.Postgres;
using ModularMonolith.Shared.Messaging.MassTransit.RabbitMQ;

namespace ModularMonolith.Infrastrucutre.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly[] assemblies)
    {
        var messagingSection = configuration.GetSection("Messaging");
        var provider = messagingSection.GetSection("Provider").Value;

        serviceCollection.AddMessageBus();
        
        switch (provider)
        {
            case "Postgres":
                serviceCollection.AddPostgresMessaging<DbContext>(configuration, assemblies);
                break;
            case "Kafka":
                serviceCollection.AddKafkaMessaging<DbContext>(configuration, assemblies);
                break;
            case "RabbitMq":
                serviceCollection.AddRabbitMQMessaging<DbContext>(configuration, assemblies);
                break;
            default:
                throw new ArgumentException("Messaging:Provider is required");
        }
        
        return serviceCollection;
    }
}
