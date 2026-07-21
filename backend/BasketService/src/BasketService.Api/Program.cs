using BasketService.Api.Middleware;
using BasketService.Application.Abstractions;
using BasketService.Persistence;
using BasketService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── OpenAPI & Scalar ───────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

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

// ── Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// ── Mediator (CQRS) ────────────────────────────────────────────────────────
builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

// ── Authentication & Authorization ────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthenticationContext, HttpContextAuthenticationContext>();

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

// ── Global Exception Middleware ───────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

// ── JWT Authentication Middleware ─────────────────────────────────────────
app.UseMiddleware<JwtAuthenticationMiddleware>();

app.UseAuthorization();

// ── Health Check Endpoints ─────────────────────────────────────────────────
app.MapHealthChecks("/health/live");     // Liveness probe
app.MapHealthChecks("/health/ready");    // Readiness probe

app.MapControllers();

app.Run();
