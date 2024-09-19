using Microsoft.OpenApi.Models;

namespace ModularMonolith.Shared.RestApi.Swagger;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerWithBearerToken(this IServiceCollection serviceCollection,
        List<IWebAppModule> modules)
    {
        serviceCollection.AddEndpointsApiExplorer()
            .AddSwaggerGen(opt =>
            {
                opt.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                            Scheme = "OAuth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                foreach (var module in modules)
                {
                    module.SwaggerGenAction(opt);
                }
            });

        return serviceCollection;
    }
}
