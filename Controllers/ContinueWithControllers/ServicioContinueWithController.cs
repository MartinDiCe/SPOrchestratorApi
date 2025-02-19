using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.DTOs.ContinueWithDtos;
using SPOrchestratorAPI.Services.ContinueWithServices;

namespace SPOrchestratorAPI.Controllers.ContinueWithControllers
{
    /// <summary>
    /// Controlador para gestionar los mapeos de continuación de procesos.
    /// Se encarga únicamente de recibir solicitudes HTTP y delegar la lógica de negocio al servicio.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ServicioContinueWithController(IServicioContinueWithService continueWithService) : ControllerBase
    {
        private readonly IServicioContinueWithService _continueWithService = continueWithService ?? throw new ArgumentNullException(nameof(continueWithService));

        /// <summary>
        /// Crea un nuevo mapeo de continuación.
        /// </summary>
        /// <param name="dto">DTO con los datos necesarios para crear el mapeo.</param>
        /// <returns>El mapeo creado.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateServicioContinueWithDto dto)
        {
            var created = await _continueWithService.CreateAsync(dto).FirstAsync();
            return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
        }

        /// <summary>
        /// Actualiza un mapeo de continuación existente.
        /// </summary>
        /// <param name="dto">DTO con los datos necesarios para actualizar el mapeo.</param>
        /// <returns>El mapeo actualizado.</returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateServicioContinueWithDto dto)
        {
            var updated = await _continueWithService.UpdateAsync(dto).FirstAsync();
            return Ok(updated);
        }

        /// <summary>
        /// Obtiene un mapeo de continuación por su identificador.
        /// </summary>
        /// <param name="id">Identificador del mapeo.</param>
        /// <returns>El mapeo correspondiente.</returns>
        [HttpGet("getbyid/{id}")]
        [ActionName("GetByIdAsync")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var mapping = await _continueWithService.GetByIdAsync(id).FirstAsync();
            return Ok(mapping);
        }

        /// <summary>
        /// Obtiene todos los mapeos de continuación no eliminados.
        /// </summary>
        /// <returns>Lista de mapeos.</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var list = await _continueWithService.GetAllAsync().FirstAsync();
            return Ok(list);
        }

        /// <summary>
        /// Obtiene los mapeos de continuación asociados a una configuración de servicio inicial.
        /// </summary>
        /// <param name="servicioConfiguracionId">Identificador de la configuración del servicio inicial.</param>
        /// <returns>Lista de mapeos asociados.</returns>
        [HttpGet("byconfig/{servicioConfiguracionId}")]
        public async Task<IActionResult> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId)
        {
            var list = await _continueWithService.GetByServicioConfiguracionIdAsync(servicioConfiguracionId).FirstAsync();
            return Ok(list);
        }

        /// <summary>
        /// Aplica soft delete a un mapeo de continuación.
        /// </summary>
        /// <param name="id">Identificador del mapeo a eliminar.</param>
        /// <returns>El mapeo eliminado.</returns>
        [HttpPut("softdelete/{id}")]
        public async Task<IActionResult> SoftDeleteAsync(int id)
        {
            var deleted = await _continueWithService.SoftDeleteAsync(id).FirstAsync();
            return Ok(deleted);
        }

        /// <summary>
        /// Restaura un mapeo de continuación previamente eliminado.
        /// </summary>
        /// <param name="id">Identificador del mapeo a restaurar.</param>
        /// <returns>El mapeo restaurado.</returns>
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreAsync(int id)
        {
            var restored = await _continueWithService.RestoreAsync(id).FirstAsync();
            return Ok(restored);
        }
    }
}
