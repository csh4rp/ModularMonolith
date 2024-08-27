﻿using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Identity.Api.IntegrationTests.Shared;

public class RabbitMessagingFixture<T> : IAsyncDisposable where T : class
{
    private readonly IServiceProvider _serviceProvider;

    private T? _message;
    private IBusControl? _busControl;

    public RabbitMessagingFixture(string connectionString, string queue)
    {
        var services = new ServiceCollection();

        services.AddMassTransit(c =>
        {
            c.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(connectionString);
                configurator.ConfigureEndpoints(context);

                configurator.ReceiveEndpoint(queue, e =>
                {
                    e.Handler<T>(consumeContext =>
                    {
                        _message = consumeContext.Message;
                        return Task.CompletedTask;
                    });

                    e.Bind<T>();
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
