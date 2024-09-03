using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Messaging.MassTransit.Filters;

namespace ModularMonolith.Tests.Utils.RabbitMQ;

public class RabbitMQMessagingFixture<T> : IAsyncDisposable where T : class
{
    private readonly IServiceProvider _serviceProvider;

    private T? _message;
    private IBusControl? _busControl;

    public RabbitMQMessagingFixture(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddMassTransit(c =>
        {
            c.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(connectionString);
                configurator.ConfigureEndpoints(context);

                configurator.ReceiveEndpoint(e =>
                {
                    e.UseConsumeFilter<IdentityFilter<T>>(context);
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
