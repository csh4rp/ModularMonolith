using System.Threading.Channels;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventChannel : IEventChannel
{
    private readonly Channel<EventInfo> _channel = Channel.CreateBounded<EventInfo>(100);

    public ChannelWriter<EventInfo> Writer => _channel.Writer;

    public ChannelReader<EventInfo> Reader => _channel.Reader;
}
