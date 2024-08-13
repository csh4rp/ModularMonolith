using MassTransit;

namespace ModularMonolith.Shared.TestUtils.Messaging;

public class TestBus
{
    private IBus? _bus;

    public async Task StartAsync(string connectionString)
    {
        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(connectionString);
        });

        await busControl.StartAsync();

        _bus = busControl;
    }

    public void ConnectReceiveEndpoint<T>(string queueName)
        where T : class, IConsumer
    {
        if (_bus is null)
        {
            throw new InvalidOperationException("Bus has to be stared before it can connect receive endpoint");
        }


        _bus.ConnectReceiveEndpoint(queueName, x =>
        {
            x.Consumer(() => (T)Activator.CreateInstance(typeof(T))!, a =>
            {
            });
        });
    }

    public void ConnectReceiveEndpoint<T>(string queueName, IConsumer<T> consumer) where T : class
    {
        if (_bus is null)
        {
            throw new InvalidOperationException("Bus has to be stared before it can connect receive endpoint");
        }

        _bus.ConnectReceiveEndpoint(queueName, x =>
        {
            x.Consumer(() => consumer, a =>
            {
            });
        });
    }
}
