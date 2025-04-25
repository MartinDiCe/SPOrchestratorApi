using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Services.HangFireServices;

namespace SPOrchestratorAPI.Controllers.HangFireControllers;

[ApiController]
[Route("api/[controller]")]
public class HangfireAdminController(
    IHangfireJobService jobs,
    ILogger<HangfireAdminController> log)
    : ControllerBase
{
    private readonly IHangfireJobService           _jobs = jobs  ?? throw new ArgumentNullException(nameof(jobs));
    private readonly ILogger<HangfireAdminController> _log = log   ?? throw new ArgumentNullException(nameof(log));

    /// <summary>Refresca todos los recurring jobs de Hangfire.</summary>
    [HttpPost("refresh-jobs")]
    public IActionResult RefreshJobs()
    {
        _log.LogInformation("🔄 Petición de refresco de recurring jobs");
        _jobs.RefreshAllRecurringJobs();
        return Ok(new { mensaje = "Recurring jobs refrescados correctamente." });
    }
}