using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.DTOs.ServicioConfiguracionDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;

namespace SPOrchestratorAPI.Controllers.ServicioConfiguracionControllers
{
    /// <summary>
    /// Controlador para gestionar la configuración de los <see cref="Servicio"/>.
    /// </summary>
    [ApiController]
    [Route("api/ServicioConfiguracion")]
    public class ServicioConfiguracionController(IServicioConfiguracionService service) : ControllerBase
    {
        private readonly IServicioConfiguracionService _service = service ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Crea una nueva configuración de servicio.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateServicioConfiguracionDto dto)
        {
            var created = await _service.CreateAsync(dto).FirstAsync();
            // Retorna 201 Created
            return CreatedAtRoute("GetByIdConfig", new { id = created.Id }, created);
        }

        /// <summary>
        /// Obtiene una configuración por ID.
        /// </summary>
        [HttpGet("{id}", Name = "GetByIdConfig")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var config = await _service.GetByIdAsync(id).FirstAsync();
            return Ok(config);
        }

        /// <summary>
        /// Actualiza una configuración existente.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateServicioConfiguracionDto dto)
        {
            var updated = await _service.UpdateAsync(dto).FirstAsync();
            return Ok(updated);
        }

        /// <summary>
        /// Elimina lógicamente una configuración (soft delete).
        /// </summary>
        [HttpPut("softdelete/{id}")]
        public async Task<IActionResult> SoftDeleteAsync(int id)
        {
            var deleted = await _service.SoftDeleteAsync(id).FirstAsync();
            return Ok(deleted);
        }

        /// <summary>
        /// Restaura una configuración previamente eliminada.
        /// </summary>
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreAsync(int id)
        {
            var restored = await _service.RestoreAsync(id).FirstAsync();
            return Ok(restored);
        }

        /// <summary>
        /// Obtiene todas las configuraciones (no eliminadas).
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var all = await _service.GetAllAsync().FirstAsync();
            return Ok(all);
        }

        /// <summary>
        /// Obtiene las configuraciones asociadas a un servicio (por ID).
        /// </summary>
        [HttpGet("byservicio/{servicioId}")]
        public async Task<IActionResult> GetByServicioIdAsync(int servicioId)
        {
            var configs = await _service.GetByServicioIdAsync(servicioId).FirstAsync();
            return Ok(configs);
        }
    }
}
