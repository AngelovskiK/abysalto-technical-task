using Microsoft.AspNetCore.Mvc;

namespace BasketService.Api.Controllers;

/// <summary>
/// Health check endpoints for Kubernetes probes and monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Liveness probe — is the service alive and responsive?
    /// </summary>
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe — is the service ready to accept traffic?
    /// Checks external dependencies (DB, Redis, Service Bus when added).
    /// </summary>
    [HttpGet("ready")]
    public IActionResult Ready()
    {
        // TODO: Add actual dependency checks (DB, Redis) once connected
        return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }
}
