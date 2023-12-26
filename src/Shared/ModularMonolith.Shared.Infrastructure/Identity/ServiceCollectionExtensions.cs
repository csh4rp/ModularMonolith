using ModularMonolith.Shared.Application.Identity;

namespace ModularMonolith.Shared.Infrastructure.Identity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IdentityContextAccessor>()
            .AddScoped<IIdentityContextSetter>(sp => sp.GetRequiredService<IdentityContextAccessor>())
            .AddScoped<IIdentityContextAccessor>(sp => sp.GetRequiredService<IdentityContextAccessor>());

        return serviceCollection;
    }
}
