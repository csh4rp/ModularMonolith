using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Events;
using ModularMonolith.Shared.TestUtils.Messaging;

namespace ModularMonolith.Identity.Api.IntegrationTests.Shared;

public class KafkaTestBus
{
    private IServiceProvider _serviceProvider;

    public KafkaTestBus(string cs, Type[] consumers)
    {
        var services = new ServiceCollection();

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            x.AddRider(cfg =>
            {
                cfg.AddConsumer<TestConsumer<PasswordChangedEvent>>();

                cfg.SetDefaultEndpointNameFormatter();

                cfg.UsingKafka((context, configurator) =>
                {
                    configurator.Host(cs);

                    configurator.TopicEndpoint<string, PasswordChangedEvent>("PasswordChanged", "gr", e =>
                    {
                        e.ConfigureConsumer<TestConsumer<PasswordChangedEvent>>(context);
                    });
                });
            });
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task StartAsync()
    {
        var busControl = _serviceProvider.GetRequiredService<IBusControl>();

        await busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
    }

    private static void SetupConsumers(IRiderRegistrationContext context,
        IKafkaFactoryConfigurator cfg,
        Type[] types)
    {
        var consumerMessages = types
            .GroupBy(t =>
            {
                var @interface = t.GetInterfaces()
                    .Single(i => i.IsGenericType && i.IsAssignableTo(typeof(IConsumer)));

                return @interface.GenericTypeArguments[0];
            })
            .ToDictionary(t => t.Key, t => t.ToList());

        foreach (var (messageType, _) in consumerMessages)
        {
            var topic = messageType.Name;

            var groupedConsumers = consumerMessages.Values.SelectMany(s => s)
                .GroupBy(t => t.GetCustomAttribute<EventConsumerAttribute>()?.ConsumerName ?? topic)
                .ToDictionary(t => t.Key, t => t.ToList());

            foreach (var (consumerGroupName, consumerTypes) in groupedConsumers)
            {
                cfg.TopicEndpoint<object>(topic, consumerGroupName, cf =>
                {
                    foreach (var consumerType in consumerTypes)
                    {
                        cf.ConfigureConsumer(context, consumerType);
                    }
                });
            }
        }
    }
}
