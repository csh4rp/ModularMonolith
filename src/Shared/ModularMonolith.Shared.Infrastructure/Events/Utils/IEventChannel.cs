using System.Threading.Channels;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal interface IEventChannel
{
    ChannelWriter<EventInfo> Writer { get; }

    ChannelReader<EventInfo> Reader { get; }
}
