using BasketService.Api.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace BasketService.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureDeveloperTools(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseCors(IServiceCollectionExtensions.FrontendCorsPolicy);
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<JwtAuthenticationMiddleware>();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
