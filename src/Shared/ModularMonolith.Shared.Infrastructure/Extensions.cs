using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ModularMonolith.Shared.Infrastructure.IntegrationTests")]
[assembly: InternalsVisibleTo("ModularMonolith.Shared.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("ModularMonolith.Shared.Migrations")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ModularMonolith.Shared.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddMass(this IServiceCollection serviceCollection)
    {



        return serviceCollection;
    }
}
