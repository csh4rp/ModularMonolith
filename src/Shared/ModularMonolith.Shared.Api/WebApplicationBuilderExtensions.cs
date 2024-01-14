using System.Reflection;
using System.Text;
using Asp.Versioning;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModularMonolith.Bootstrapper.Infrastructure.DataAccess;
using ModularMonolith.Shared.Api.Exceptions;
using ModularMonolith.Shared.Api.Middlewares;
using ModularMonolith.Shared.Application;
using ModularMonolith.Shared.Domain.Attributes;
using ModularMonolith.Shared.Infrastructure.AuditLogs;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events;
using ModularMonolith.Shared.Infrastructure.Identity;
using ModularMonolith.Shared.Infrastructure.Messaging;
using Npgsql;
using IConsumer = MassTransit.IConsumer;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularMonolith.Shared.Api;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder RegisterModules(this WebApplicationBuilder builder)
    {
        var modules = builder.Configuration.GetEnabledModules().ToList();

        foreach (var module in modules)
        {
            builder.Services.AddSingleton<AppModule>(_ => (AppModule)Activator.CreateInstance(module.GetType())!);
        }

        var assemblies = modules.SelectMany(m => m.Assemblies).ToArray();

        var authType = builder.Configuration.GetSection("Authentication:Type").Get<string>()
                       ?? throw new ArgumentException("'Authentication:Type' is not configured");

        if (authType.Equals("Bearer"))
        {
            var key = builder.Configuration.GetSection("Authentication:SigningKey").Get<string>()
                      ?? throw new ArgumentException("'Authentication:SigningKey' is not configured");

            var audience = builder.Configuration.GetSection("Authentication:Audience").Get<string>()
                           ?? throw new ArgumentException("'Authentication:Audience' is not configured");

            var issuer = builder.Configuration.GetSection("Authentication:Issuer").Get<string>()
                         ?? throw new ArgumentException("'Authentication:Issuer' is not configured");

            builder.Services.AddAuthentication(options =>
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
                });
        }
        else
        {
            throw new NotSupportedException("Other auth types are not supported yet");
        }

        builder.Services.AddScoped<IdentityMiddleware>();

        builder.Services.AddAuthorization(options =>
        {
        });

        var connectionString = builder.Configuration.GetConnectionString("Database");
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        
        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.Host = connectionStringBuilder.Host;
            options.Database = connectionStringBuilder.Database;
            options.Schema = "shared";
            options.Role = "shared";
            options.Username = connectionStringBuilder.Username;
            options.Password = connectionStringBuilder.Password;
        });

        builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, optionsBuilder) =>
        {
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseSnakeCaseNamingConvention();
            optionsBuilder.AddInterceptors(new AuditLogInterceptor());
            optionsBuilder.UseApplicationServiceProvider(sp);
        }, ServiceLifetime.Scoped);

        builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddPostgresMigrationHostedService(true, false);
        
        builder.Services.AddMassTransit(c =>
        {
            c.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();
                
                o.UseBusOutbox(a =>
                {
                    a.MessageDeliveryLimit = 10;
                });
            });
            
            c.AddConsumers(modules.SelectMany(m => m.Assemblies).ToArray());

            c.AddConfigureEndpointsCallback((context, _, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<ApplicationDbContext>(context);
            });
            
            
            c.UsingPostgres((context, cfg) =>
            {
                cfg.AutoStart = true;
                cfg.UseDbMessageScheduler();
                
                cfg.MessageTopology.SetEntityNameFormatter(new EventAttributeEntityNameFormatter());
                
                var consumerMessages = modules.SelectMany(m => m.Assemblies)
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsAssignableTo(typeof(IConsumer)))
                    .GroupBy(t =>
                    {
                        var @interface = t.GetInterfaces()
                            .Single(i => i.IsGenericType && i.IsAssignableTo(typeof(IConsumer)));
                        
                        return @interface.GenericTypeArguments[0];
                    })
                    .ToDictionary(t => t.Key, t => t.ToList());
                
                foreach (var messageType in consumerMessages.Keys)
                {
                    cfg.Publish(messageType, msg =>
                    {
                        
                    });
                }

                foreach (var (messageType, _) in consumerMessages)
                {
                    var eventAttribute = messageType.GetCustomAttribute<EventAttribute>()!;
                    var topic = eventAttribute.Topic ?? messageType.Name;

                    var groupedConsumers = consumerMessages.Values.SelectMany(s => s)
                        .GroupBy(t => t.GetCustomAttribute<EventConsumerAttribute>()?.Queue ?? topic)
                        .ToDictionary(t => t.Key, t => t.ToList());

                    foreach (var (queue, consumerTypes) in groupedConsumers)
                    {
                        cfg.ReceiveEndpoint(queue, cf =>
                        {
                            foreach (var consumerType in consumerTypes)
                            {
                                cf.ConfigureConsumer(context, consumerType);
                            }
                            
                            cf.Subscribe(topic);
                        });
                    }
                }
            });
            
        });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
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
        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new MediaTypeApiVersionReader();
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        builder.Services.AddMediator(assemblies);
        builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        builder.Services.AddEvents(assemblies);

        builder.Services
            .AddSingleton(TimeProvider.System)
            .AddIdentityServices()
            .AddHttpContextAccessor()
            .AddExceptionHandlers();
        
        builder.Services.AddOpenTelemetry()
            .WithTracing(b =>
            {
                b.AddSource("ModularMonolith")
                    .ConfigureResource(resource =>
                        resource.AddService(
                            serviceName: "ModularMonolith",
                            serviceVersion: "1.0.0"))
                    .AddAspNetCoreInstrumentation();

                if (builder.Environment.IsDevelopment())
                {
                    b.AddConsoleExporter();
                }
            });

        builder.Services.AddDataAccess(c =>
        {
            c.ConnectionString = builder.Configuration.GetConnectionString("Database")!;
        });

        builder.Services.AddAuditLogs();

        foreach (var module in modules)
        {
            module.RegisterServices(builder.Services);
        }

        return builder;
    }
}
