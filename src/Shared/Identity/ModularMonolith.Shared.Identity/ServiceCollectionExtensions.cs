using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Identity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityContextAccessor(this IServiceCollection services)
    {
        services.AddScoped<IdentityContextWrapper>();
        services.AddScoped<IIdentityContextAccessor>(sp => sp.GetRequiredService<IdentityContextWrapper>());
        services.AddScoped<IIdentityContextSetter>(sp => sp.GetRequiredService<IdentityContextWrapper>());

        return services;
    }
}
