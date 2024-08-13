namespace ModularMonolith.Shared.TestUtils.Messaging;

public class MessagePublicationVerifier<T> where T : class
{
    private readonly TestConsumer<T> _testConsumer;

    public MessagePublicationVerifier(TestConsumer<T> testConsumer) => _testConsumer = testConsumer;

    public async Task<T> VerifyAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var token = cts.Token;

        while (!token.IsCancellationRequested)
        {
            if (_testConsumer.Message is not null)
            {
                return _testConsumer.Message;
            }

            await Task.Delay(10, token);
        }

        throw new Exception("Message was not received within specified period of time");
    }
}
