﻿using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ModularMonolith.Shared.RestApi.Authorization;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var authType = configuration.GetSection("Authentication:Type").Get<string>()
                       ?? throw new ArgumentException("'Authentication:Type' is not configured");

        serviceCollection.AddAuthorization();

        if (authType.Equals("Bearer"))
        {
            var key = configuration.GetSection("Authentication:SigningKey").Get<string>()
                      ?? throw new ArgumentException("'Authentication:SigningKey' is not configured");

            var audience = configuration.GetSection("Authentication:Audience").Get<string>()
                           ?? throw new ArgumentException("'Authentication:Audience' is not configured");

            var issuer = configuration.GetSection("Authentication:Issuer").Get<string>()
                         ?? throw new ArgumentException("'Authentication:Issuer' is not configured");

            serviceCollection.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = audience,
                        ValidIssuer = issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        RequireAudience = true,
                        RequireSignedTokens = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true
                    };

                    options.MapInboundClaims = false;
                });
        }
        else if (authType.Equals("OAuth"))
        {
            var clientId = configuration.GetSection("Authentication:ClientId").Get<string>()
                           ?? throw new ArgumentException("Authentication:ClientId is not configured");

            var clientSecret = configuration.GetSection("Authentication:ClientSecret").Get<string>()
                           ?? throw new ArgumentException("Authentication:ClientSecret is not configured");

            serviceCollection.AddAuthentication()
                .AddOAuth("", authOptions =>
                {
                    authOptions.ClientId = clientId;
                    authOptions.ClientSecret = clientSecret;
                });
        }
        else
        {
            throw new NotSupportedException("Other auth types are not supported yet");
        }

        return serviceCollection;
    }
}
