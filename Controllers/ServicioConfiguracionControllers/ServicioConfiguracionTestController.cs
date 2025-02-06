using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.DTOs.StoreProcedureDtos;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.StoreProcedureServices;

namespace SPOrchestratorAPI.Controllers.ServicioConfiguracionControllers
{
    /// <summary>
    /// Controlador para testear la conexión de una configuración de servicio.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ServicioConfiguracionTestController : ControllerBase
    {
        private readonly IServicioConfiguracionConnectionTestService _testService;
        private readonly IStoredProcedureService _spService;

        /// <summary>
        /// Constructor que recibe la dependencia del servicio de testeo de conexión.
        /// </summary>
        /// <param name="testService">Servicio para testear la conexión de la configuración.</param>
        public ServicioConfiguracionTestController(IServicioConfiguracionConnectionTestService testService, IStoredProcedureService spService)
        {
            _testService = testService ?? throw new ArgumentNullException(nameof(testService));
            _spService = spService ?? throw new ArgumentNullException(nameof(spService));

        }

        /// <summary>
        /// Endpoint para testear la conexión de una configuración a partir de su ID.
        /// </summary>
        /// <param name="id">ID de la configuración de servicio.</param>
        /// <returns>
        /// En caso de éxito, retorna un 200 OK con el resultado del test de conexión.
        /// Si no se encuentra la configuración, retorna 404 Not Found.
        /// En caso de error interno, retorna 500 Internal Server Error.
        /// </returns>
        [HttpGet("test/{id}")]
        public async Task<IActionResult> TestConnection(int id)
        {
            try
            {
                
                var result = await _testService.TestConnectionAsync(id);
                
                return Ok(result);
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al probar la conexión.", detail = ex.Message });
            }
        }
        
        /// <summary>
        /// Endpoint para ejecutar un stored procedure utilizando la configuración específica.
        /// El request solo debe incluir el ID de la configuración y los valores de los parámetros; el nombre del SP se obtiene de la configuración.
        /// </summary>
        /// <param name="request">Objeto con el ID de la configuración y los parámetros (opcional).</param>
        /// <returns>El número de filas afectadas por la ejecución del SP.</returns>
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteSp([FromBody] StoredProcedureTestRequest request)
        {
            try
            {
                var rowsAffected = await _spService.EjecutarSpAsync(request.IdConfiguracion, request.Parameters).FirstAsync();
                return Ok(new { RowsAffected = rowsAffected });
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al ejecutar el stored procedure.", detail = ex.Message });
            }
        }
    }
}
