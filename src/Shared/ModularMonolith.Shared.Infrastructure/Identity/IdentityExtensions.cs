using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Identity;

namespace ModularMonolith.Shared.Infrastructure.Identity;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentity(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IdentityContextAccessor>()
            .AddScoped<IIdentityContextSetter>(sp => sp.GetRequiredService<IdentityContextAccessor>())
            .AddScoped<IIdentityContextAccessor>(sp => sp.GetRequiredService<IdentityContextAccessor>());
        
        return serviceCollection;
    } 
}
