using Microsoft.AspNetCore.Mvc;

namespace SPOrchestratorAPI.Controllers.AdminControllers;

[ApiController]
[Route("api/Admin")]
public class AdminController(
    IHostApplicationLifetime appLifetime,
    ILogger<AdminController> logger)
    : ControllerBase
{
    /// <summary>
    /// Detiene la aplicación para que el host (por ejemplo IIS/Service) la vuelva a iniciar.
    /// </summary>
    [HttpPost("restart")]
    public IActionResult Restart()
    {
        logger.LogWarning("Aplicación reiniciándose por petición administrativa...");
        
        Task.Run(() => {
            
            Thread.Sleep(500);
            appLifetime.StopApplication();
        });
        return Ok(new { mensaje = "Reinicio iniciado." });
    }
}
