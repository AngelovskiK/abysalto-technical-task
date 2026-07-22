using BasketService.Api.Middleware;
using BasketService.Application.Abstractions;
using BasketService.Application.Behaviors;
using BasketService.Application.Settings;
using BasketService.Persistence;
using BasketService.Persistence.Caching;
using BasketService.Persistence.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace BasketService.Api.Extensions;

public static class IServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "Frontend";

    public static IServiceCollection ConfigureConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
        return services;
    }

    public static IServiceCollection ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Enter token as: Bearer {your JWT token}"
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                var relativePath = context.Description.RelativePath;
                if (!string.IsNullOrWhiteSpace(relativePath) &&
                    relativePath.StartsWith("api/cart", StringComparison.OrdinalIgnoreCase))
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", null!, null)] = []
                    });
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddSource("Npgsql");
                tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddOtlpExporter();
            });

        return services;
    }

    public static IServiceCollection ConfigureData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BasketDb")
            ?? throw new InvalidOperationException("Connection string 'BasketDb' not found.");

        services.AddDbContext<BasketDbContext>(options => options.UseNpgsql(connectionString));

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddScoped<ICartCache, RedisCartCache>();

        services
            .AddHealthChecks()
            .AddDbContextCheck<BasketDbContext>(name: "database", tags: ["ready"])
            .AddRedis(redisConnectionString, name: "redis", tags: ["ready"]);

        return services;
    }

    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();

        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

        var applicationAssembly = typeof(ValidationBehavior<,>).Assembly;
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddScoped(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    public static IServiceCollection ConfigureAuthAndCors(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthenticationContext, HttpContextAuthenticationContext>();

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                policy
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.PermitLimit = 4;
                opt.Window = TimeSpan.FromSeconds(12);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });
        });

        return services;
    }
}
