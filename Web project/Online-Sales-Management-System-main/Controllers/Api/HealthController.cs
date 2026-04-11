using Microsoft.AspNetCore.Mvc;

namespace OnlineSalesManagementSystem.Controllers.Api;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "Online Sales Management System",
            serverTimeUtc = DateTime.UtcNow,
            version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
        });
    }
}
