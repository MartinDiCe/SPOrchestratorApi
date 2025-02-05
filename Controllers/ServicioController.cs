using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.DTOs;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services;

namespace SPOrchestratorAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar operaciones CRUD sobre la entidad <see cref="Servicio"/>.
    /// </summary>
    [ApiController]
    [Route("api/Servicio")]
    public class ServicioController : ControllerBase
    {
        private readonly IServicioService _servicioService;

        /// <summary>
        /// Constructor que inyecta la capa de servicio para <see cref="Servicio"/>.
        /// </summary>
        /// <param name="servicioService">Servicio que contiene la lógica de negocio de <see cref="Servicio"/>.</param>
        public ServicioController(IServicioService servicioService)
        {
            _servicioService = servicioService 
                ?? throw new ArgumentNullException(nameof(servicioService));
        }

        /// <summary>
        /// Crea un nuevo <see cref="Servicio"/> si no existe otro con el mismo nombre (no eliminado).
        /// </summary>
        /// <param name="dto">Datos necesarios para crear el servicio.</param>
        /// <returns>
        /// 201 Created si la creación es exitosa, 409 Conflict si el nombre está duplicado,
        /// o 400 BadRequest si alguno de los datos es inválido.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateServicioAsync([FromBody] CreateServicioDto dto)
        {
            var created = await _servicioService.CreateAsync(dto).FirstAsync();

            return CreatedAtAction("GetServicioById",
                new { id = created.Id },
                created
            );
        }

        /// <summary>
        /// Obtiene un servicio por su identificador (ID).
        /// </summary>
        /// <param name="id">Identificador del servicio a buscar.</param>
        /// <returns>El servicio con el ID especificado.</returns>
        [HttpGet("getbyid/{id}", Name = "GetServicioById")]
        public async Task<IActionResult> GetServicioByIdAsync(int id)
        {
            var service = await _servicioService
                .GetByIdAsync(id)
                .FirstAsync();

            return Ok(service);
        }

        /// <summary>
        /// Obtiene un servicio por su nombre.
        /// </summary>
        /// <param name="name">Nombre del servicio a buscar.</param>
        /// <returns>El servicio con el nombre especificado.</returns>
        [HttpGet("getbyname/{name}")]
        public async Task<IActionResult> GetServicioByNameAsync(string name)
        {
            var service = await _servicioService
                .GetByNameAsync(name)
                .FirstAsync();

            return Ok(service);
        }
    }
}