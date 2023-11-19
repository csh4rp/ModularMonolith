using System.Reflection;

namespace ModularMonolith.Shared.Infrastructure.Messaging.Options;

public class MessagingOptions
{
    public Uri HostUri { get; set; } = default!;

    public string Username { get; set; } = default!;

    public string Password { get; set; } = default!;
    
    public List<Assembly> Assemblies { get; set; } = new();
}
