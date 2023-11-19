using System.Reflection;

namespace ModularMonolith.Shared.Infrastructure.Messaging;

public class MessagingOptions
{
    public Uri Uri { get; set; }
    
    public List<Assembly> Assemblies { get; set; } = new();
}
