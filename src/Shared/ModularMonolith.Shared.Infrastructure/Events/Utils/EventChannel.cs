using System.Threading.Channels;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Options;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventChannel : IEventChannel
{
    private readonly Channel<EventInfo> _channel;
    
    public EventChannel(IOptionsMonitor<EventOptions> optionsMonitor) => 
        _channel = Channel.CreateBounded<EventInfo>(optionsMonitor.CurrentValue.MaxEventChannelSize);

    public ChannelWriter<EventInfo> Writer => _channel.Writer;

    public ChannelReader<EventInfo> Reader => _channel.Reader;
}
