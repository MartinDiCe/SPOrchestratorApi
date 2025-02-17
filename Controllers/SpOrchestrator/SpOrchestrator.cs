using System.Reactive.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.DTOs.StoreProcedureDtos;
using SPOrchestratorAPI.Services.SPOrchestratorServices;

namespace SPOrchestratorAPI.Controllers.SpOrchestrator
{
    /// <summary>
    /// Controlador para la ejecución final de stored procedures.
    /// Se recibe el nombre del servicio y los valores de los parámetros.
    /// La configuración (nombre del SP, cadena de conexión, proveedor y parámetros esperados)
    /// se obtiene de la base de datos.
    /// </summary>
    [ApiExplorerSettings(GroupName = "Public")]
    [ApiController]
    [Route("api/[controller]")]
    public class SpOrchestrator(ISpOrchestratorService spService) : ControllerBase
    {
        private readonly ISpOrchestratorService _spService =
            spService ?? throw new ArgumentNullException(nameof(spService));

        /// <summary>
        /// Endpoint para ejecutar un stored procedure utilizando la configuración obtenida por el nombre del servicio.
        /// Se espera que el request incluya el nombre del servicio y el diccionario de parámetros.
        /// </summary>
        /// <param name="request">DTO que contiene el nombre del servicio y los valores de los parámetros.</param>
        /// <returns>
        /// En caso de éxito, retorna un 200 OK con el resultado del SP (por ejemplo, un listado de filas en formato JSON).
        /// En caso de error, retorna un mensaje con el detalle del error.
        /// </returns>
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteSp([FromBody] StoredProcedureExecutionRequest request)
        {
            try
            {
                var result = await _spService
                    .EjecutarPorNombreAsync(request.ServiceName, request.Parameters)
                    .FirstAsync();

                if (request.IsFile)
                {
                    if (result is List<Dictionary<string, object>> listResult)
                    {
                        var csvContent = CsvConverter.ConvertToCsv(listResult);
                        var bytes = Encoding.UTF8.GetBytes(csvContent);
                        return File(bytes, "text/csv", "resultado.csv");
                    }
                    else
                    {
                        return BadRequest(new
                            { message = "El stored procedure no devolvió un contenido adecuado para un archivo." });
                    }
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al ejecutar el stored procedure.", detail = ex.Message });
            }
        }
    }
}