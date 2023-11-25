using System.Threading.Channels;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventChannel
{
    private readonly Channel<Guid> _channel = Channel.CreateBounded<Guid>(1000);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken) => 
        _channel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask WriteAsync(Guid id, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(id, cancellationToken);

    public bool TryWrite(Guid id) => _channel.Writer.TryWrite(id);
}
