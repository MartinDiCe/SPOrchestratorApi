using System.Reactive.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.DTOs.StoreProcedureDtos;
using SPOrchestratorAPI.Services.ChainOrchestratorServices;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Controllers.SpOrchestrator
{
    /// <summary>
    /// Controlador para orquestar la ejecución de servicios (SP, vistas, endpoints)
    /// según la configuración almacenada en BD.
    /// </summary>
    [ApiExplorerSettings(GroupName = "Public")]
    [ApiController]
    [Route("api/[controller]")]
    public class SpOrchestrator : ControllerBase
    {
        private readonly IChainOrchestratorService _spService;
        private readonly ILoggerService<SpOrchestrator> _logger;

        /// <summary>
        /// Constructor de <see cref="SpOrchestrator"/>.
        /// </summary>
        /// <param name="spService">Servicio que implementa la lógica reactiva de orquestación.</param>
        /// <param name="logger">Servicio genérico de logging.</param>
        public SpOrchestrator(
            IChainOrchestratorService spService,
            ILoggerService<SpOrchestrator> logger)
        {
            _spService = spService
                ?? throw new ArgumentNullException(nameof(spService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Ejecuta un servicio orquestado (SP, vista o endpoint) y devuelve todos los resultados.
        /// </summary>
        /// <param name="request">
        /// DTO con <see cref="StoredProcedureExecutionRequest.ServiceName"/>, 
        /// <see cref="StoredProcedureExecutionRequest.Parameters"/> e <c>IsFile</c>.
        /// </param>
        /// <returns>
        /// Si <c>IsFile==true</c>, un CSV descargable; 
        /// de lo contrario un 200 OK con el objeto o lista JSON.
        /// </returns>
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteSp(
            [FromBody] StoredProcedureExecutionRequest request)
        {
            _logger.LogInfo($"[SpOrchestrator] Inicio ejecución '{request.ServiceName}'");

            try
            {
                // Recolecta **todos** los valores emitidos
                var resultados = await _spService
                    .EjecutarConContinuacionAsync(request.ServiceName, request.Parameters)
                    .ToList();

                _logger.LogInfo($"[SpOrchestrator] Obtenidos {resultados.Count} resultados");

                if (request.IsFile)
                {
                    if (resultados is List<Dictionary<string, object>> listResult)
                    {
                        var csv = CsvConverter.ConvertToCsv(listResult);
                        var bytes = Encoding.UTF8.GetBytes(csv);
                        return File(bytes, "text/csv", "resultado.csv");
                    }

                    return BadRequest(new
                    {
                        message = "Contenido no adecuado para archivo."
                    });
                }

                // Si solo hay uno, lo devuelvo como objeto; si hay varios, como lista
                return resultados.Count switch
                {
                    0 => NoContent(),
                    1 => Ok(resultados[0]),
                    _ => Ok(resultados)
                };
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SpOrchestrator] Error: {ex.Message}", ex);
                return StatusCode(500, new
                {
                    message = "Error interno en orquestador.",
                    detail = ex.Message
                });
            }
        }
    }
}
