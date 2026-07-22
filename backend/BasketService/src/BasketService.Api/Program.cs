using BasketService.Api.Middleware;
using BasketService.Application.Abstractions;
using BasketService.Application.Behaviors;
using BasketService.Persistence;
using BasketService.Persistence.Caching;
using BasketService.Persistence.Repositories;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "Frontend";

// ── OpenAPI & Scalar ───────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── OpenTelemetry ─────────────────────────────────────────────────────────
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddSource("Npgsql");  // Enable PostgreSQL tracing
        tracing.AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddOtlpExporter();
    });

// ── Database (PostgreSQL) ──────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("BasketDb")
    ?? throw new InvalidOperationException("Connection string 'BasketDb' not found.");
builder.Services.AddDbContext<BasketDbContext>(options =>
    options.UseNpgsql(connectionString)
);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
redisOptions.AbortOnConnectFail = false;
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
builder.Services.AddScoped<ICartCache, RedisCartCache>();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<BasketDbContext>(name: "database", tags: ["ready"])
    .AddRedis(redisConnectionString, name: "redis", tags: ["ready"]);

// ── Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// ── Mediator (CQRS) ────────────────────────────────────────────────────────
builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

// ── FluentValidation ───────────────────────────────────────────────────────
var applicationAssembly = typeof(ValidationBehavior<,>).Assembly;
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

// Register the validation pipeline behavior
builder.Services.AddScoped(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ── Authentication & Authorization ────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthenticationContext, HttpContextAuthenticationContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();  // Scalar UI at /scalar
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);

// ── Global Exception Middleware ───────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

// ── JWT Authentication Middleware ─────────────────────────────────────────
app.UseMiddleware<JwtAuthenticationMiddleware>();

app.UseAuthorization();

// ── Health Check Endpoints ─────────────────────────────────────────────────
app.MapHealthChecks("/health/live");     // Liveness probe — no dependency checks
app.MapHealthChecks("/health/ready", new HealthCheckOptions   // Readiness — DB + Redis
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();

app.Run();

public partial class Program
{
}
