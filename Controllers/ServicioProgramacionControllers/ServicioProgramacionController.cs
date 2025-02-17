using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.DTOs.ServicioProgramacioDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;

namespace SPOrchestratorAPI.Controllers.ServicioProgramacionControllers
{
    /// <summary>
    /// Controlador para gestionar operaciones CRUD sobre la entidad <see cref="ServicioProgramacion"/>.
    /// </summary>
    [ApiController]
    [Route("api/ServicioProgramacion")]
    public class ServicioProgramacionController : ControllerBase
    {
        private readonly IServicioProgramacionService _servicioProgramacionService;

        /// <summary>
        /// Constructor que inyecta la capa de servicio para <see cref="ServicioProgramacion"/>.
        /// </summary>
        /// <param name="servicioProgramacionService">
        /// Servicio que contiene la lógica de negocio para <see cref="ServicioProgramacion"/>.
        /// </param>
        public ServicioProgramacionController(IServicioProgramacionService servicioProgramacionService)
        {
            _servicioProgramacionService = servicioProgramacionService 
                ?? throw new ArgumentNullException(nameof(servicioProgramacionService));
        }

        /// <summary>
        /// Crea una nueva programación de servicio.
        /// </summary>
        /// <param name="dto">Datos necesarios para crear la programación.</param>
        /// <returns>
        /// 201 Created si la creación es exitosa, o 400 BadRequest si alguno de los datos es inválido.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateProgramacionAsync([FromBody] CreateServicioProgramacionDto dto)
        {
            var created = await _servicioProgramacionService.CreateAsync(dto).FirstAsync();
            return CreatedAtAction("GetProgramacionById",
                new { id = created.Id },
                created);
        }

        /// <summary>
        /// Obtiene una programación por su identificador (ID).
        /// </summary>
        /// <param name="id">Identificador de la programación a buscar.</param>
        /// <returns>La programación con el ID especificado.</returns>
        [HttpGet("getbyid/{id}", Name = "GetProgramacionById")]
        public async Task<IActionResult> GetProgramacionByIdAsync(int id)
        {
            var programacion = await _servicioProgramacionService.GetByIdAsync(id).FirstAsync();
            return Ok(programacion);
        }

        /// <summary>
        /// Retorna todas las programaciones no eliminadas.
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProgramacionesAsync()
        {
            var programaciones = await _servicioProgramacionService.GetAllAsync().FirstAsync();
            return Ok(programaciones);
        }

        /// <summary>
        /// Obtiene las programaciones asociadas a una configuración de servicio específica.
        /// </summary>
        /// <param name="servicioConfiguracionId">
        /// Identificador de la configuración de servicio.
        /// </param>
        /// <returns>
        /// Una lista de programaciones asociadas a la configuración especificada.
        /// </returns>
        [HttpGet("byserviceconfig/{servicioConfiguracionId}")]
        public async Task<IActionResult> GetProgramacionesByConfigAsync(int servicioConfiguracionId)
        {
            var programaciones = await _servicioProgramacionService
                .GetByServicioConfiguracionIdAsync(servicioConfiguracionId)
                .FirstAsync();

            return Ok(programaciones);
        }

        /// <summary>
        /// Actualiza una programación existente.
        /// </summary>
        /// <param name="dto">Datos necesarios para actualizar la programación.</param>
        /// <returns>La programación actualizada.</returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProgramacionAsync([FromBody] UpdateServicioProgramacionDto dto)
        {
            var updated = await _servicioProgramacionService.UpdateAsync(dto).FirstAsync();
            return Ok(updated);
        }

        /// <summary>
        /// Elimina lógicamente una programación (soft delete).
        /// </summary>
        /// <param name="id">Identificador de la programación a eliminar.</param>
        [HttpPut("softdelete/{id}")]
        public async Task<IActionResult> SoftDeleteProgramacionAsync(int id)
        {
            var deleted = await _servicioProgramacionService.SoftDeleteAsync(id).FirstAsync();
            return Ok(deleted);
        }

        /// <summary>
        /// Restaura una programación previamente marcada como eliminada.
        /// </summary>
        /// <param name="id">Identificador de la programación a restaurar.</param>
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreProgramacionAsync(int id)
        {
            var restored = await _servicioProgramacionService.RestoreAsync(id).FirstAsync();
            return Ok(restored);
        }
    }
}