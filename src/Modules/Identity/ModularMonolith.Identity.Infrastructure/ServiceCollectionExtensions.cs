using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;

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

        serviceCollection.AddDbContextFactory<IdentityDbContext>((sp, b) =>
        {
            var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();

            b.UseNpgsql(options.Value.ConnectionString);
            b.UseSnakeCaseNamingConvention();
        });

        return serviceCollection;
    }
}
