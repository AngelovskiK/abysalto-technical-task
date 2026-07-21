using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BasketService.Application.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace BasketService.Api.Middleware;

/// <summary>
/// Middleware to extract JWT bearer token and populate IAuthenticationContext.
/// </summary>
public class JwtAuthenticationMiddleware
{
    private const string SecretKey = "dev-secret-key-change-me-in-production";
    private readonly RequestDelegate _next;

    public JwtAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthenticationContext authContext)
    {
        var token = ExtractToken(context);
        if (!string.IsNullOrEmpty(token))
        {
            var claims = ValidateAndParseClaims(token);
            if (claims != null)
            {
                // Store claims in HttpContext.Items for downstream access
                var subClaim = claims.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                var emailClaim = claims.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

                if (subClaim != null)
                    context.Items["UserId"] = subClaim;
                if (emailClaim != null)
                    context.Items["Email"] = emailClaim;
            }
        }

        await _next(context);
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

    private static ClaimsPrincipal? ValidateAndParseClaims(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(SecretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = "basket-api",
                ValidateAudience = true,
                ValidAudience = "basket-api-client",
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
                userIdObj is string userIdStr &&
                Guid.TryParse(userIdStr, out var userId))
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
