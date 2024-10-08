﻿using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Tests.Utils.Kafka;

public class KafkaMessagingFixture<T> : IAsyncDisposable where T : class
{
    private readonly IServiceProvider _serviceProvider;

    private T? _message;
    private IBusControl? _busControl;

    public KafkaMessagingFixture(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            x.AddRider(cfg =>
            {
                cfg.SetDefaultEndpointNameFormatter();

                cfg.UsingKafka((context, configurator) =>
                {
                    configurator.Host(connectionString);

                    var topicName = DefaultEndpointNameFormatter.Instance.Message<T>();

                    configurator.TopicEndpoint<string, T>(topicName, Guid.NewGuid().ToString(), e =>
                    {
                        e.Handler<T>(cnx =>
                        {
                            _message = cnx.Message;
                            return Task.CompletedTask;
                        }, a => { });
                    });
                });
            });
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task StartAsync()
    {
        _busControl = _serviceProvider.GetRequiredService<IBusControl>();

        await _busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
    }

    public async Task<T> VerifyMessageReceivedAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var token = cts.Token;

        while (!token.IsCancellationRequested)
        {
            if (_message is not null)
            {
                return _message;
            }

            await Task.Delay(10, token);
        }

        throw new Exception("Message was not received within specified period of time");
    }

    public async ValueTask DisposeAsync()
    {
        _message = null;
        await _busControl!.StopAsync();
    }
}
