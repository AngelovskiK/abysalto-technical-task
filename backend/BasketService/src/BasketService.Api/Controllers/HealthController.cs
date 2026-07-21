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
}
