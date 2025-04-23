using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Services.HangFireServices;

namespace SPOrchestratorAPI.Controllers.HangFireControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HangfireAdminController(
        IHangfireJobService jobService,
        ILogger<HangfireAdminController> logger)
        : ControllerBase
    {
        private readonly IHangfireJobService _jobService = jobService;

        /// <summary>
        /// Endpoint para refrescar todos los recurring jobs.
        /// </summary>
        [HttpPost("refresh-jobs")]
        public IActionResult RefreshJobs()
        {
            logger.LogInformation("Petición recibida: refrescar recurring jobs");
            _jobService.RefreshAllRecurringJobs();
            return Ok(new { mensaje = "Recurring jobs refrescados correctamente." });
        }
    }
}