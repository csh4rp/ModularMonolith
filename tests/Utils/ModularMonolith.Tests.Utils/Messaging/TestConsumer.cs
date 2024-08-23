using MassTransit;

namespace ModularMonolith.Shared.TestUtils.Messaging;

public class TestConsumer<T> : IDisposable, IConsumer<T> where T : class
{
    public T? Message { get; private set; }

    public Task Consume(ConsumeContext<T> context)
    {
        Message = context.Message;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Message = null;
    }
}
