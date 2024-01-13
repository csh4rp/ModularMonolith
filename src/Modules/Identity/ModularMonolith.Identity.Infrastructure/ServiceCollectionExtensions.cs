using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Infrastructure.Common.Concrete;

namespace ModularMonolith.Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        // serviceCollection.AddOptions().AddLogging();
        // serviceCollection.AddScoped<IUserValidator<User>, UserValidator<User>>();
        // serviceCollection.AddScoped<IPasswordValidator<User>, PasswordValidator<User>>();
        // serviceCollection.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        // serviceCollection.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        // serviceCollection.AddScoped<IUserConfirmation<User>, DefaultUserConfirmation<User>>();
        // serviceCollection.AddScoped<IdentityErrorDescriber>();
        // serviceCollection.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>();
        // serviceCollection.AddScoped<UserManager<User>>();
        serviceCollection.AddIdentityCore<User>()
            .AddDefaultTokenProviders();

        serviceCollection.AddDataProtection();

        serviceCollection.AddScoped<IRoleStore<Role>, RoleStore>();
        serviceCollection.AddScoped<IUserStore<User>, UserStore>();

        return serviceCollection;
    }
}
