using BasketService.Application.Abstractions;
using BasketService.Application.Errors;
using BasketService.Application.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BasketService.Api.Middleware;

/// <summary>
/// Middleware to extract JWT bearer token and populate IAuthenticationContext.
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requiresAuthentication = context.Request.Path.StartsWithSegments("/api/cart");
        var token = ExtractToken(context);
        var jwtOptions = context.RequestServices.GetRequiredService<IOptions<JwtOptions>>().Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            if (requiresAuthentication)
            {
                await WriteUnauthorizedResponseAsync(context);
                return;
            }

            await _next(context);
            return;
        }

        var claims = ValidateAndParseClaims(token, jwtOptions);
        var subClaim = claims?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var emailClaim = claims?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

        if (claims is null || !Guid.TryParse(subClaim, out var userId) || userId == Guid.Empty)
        {
            if (requiresAuthentication)
            {
                await WriteUnauthorizedResponseAsync(context);
                return;
            }

            await _next(context);
            return;
        }

        context.Items["UserId"] = userId;

        if (!string.IsNullOrWhiteSpace(emailClaim))
        {
            context.Items["Email"] = emailClaim;
        }

        await _next(context);
    }

    private static Task WriteUnauthorizedResponseAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = new UnauthorizedError().Code,
            Detail = new UnauthorizedError().Message
        });
    }

    private static string? ExtractToken(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..];
        }
        return null;
    }

    private ClaimsPrincipal? ValidateAndParseClaims(string token, JwtOptions jwtOptions)
    {
        try
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
            var tokenHandler = new JwtSecurityTokenHandler
            {
                MapInboundClaims = false
            };

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Implementation of IAuthenticationContext that reads from HttpContext.Items.
/// </summary>
public class HttpContextAuthenticationContext : IAuthenticationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAuthenticationContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("UserId", out var userIdObj) == true &&
                userIdObj is Guid userId &&
                userId != Guid.Empty)
            {
                return userId;
            }
            return null;
        }
    }

    public string? Email
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("Email", out var emailObj) == true &&
                emailObj is string email)
            {
                return email;
            }
            return null;
        }
    }
}
