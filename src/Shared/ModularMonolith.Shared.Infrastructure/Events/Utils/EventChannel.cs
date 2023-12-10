using System.Threading.Channels;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventChannel
{
    private readonly Channel<EventInfo> _channel = Channel.CreateBounded<EventInfo>(100);

    public IAsyncEnumerable<EventInfo> ReadAllAsync(CancellationToken cancellationToken) => 
        _channel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask WriteAsync(EventInfo eventInfo, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(eventInfo, cancellationToken);

    public bool TryWrite(EventInfo eventInfo) => _channel.Writer.TryWrite(eventInfo);
}
