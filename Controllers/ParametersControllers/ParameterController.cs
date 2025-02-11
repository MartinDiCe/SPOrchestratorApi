using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.DTOs.ParameterDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ParameterServices;

namespace SPOrchestratorAPI.Controllers.ParametersControllers
{
    /// <summary>
    /// Controlador para gestionar operaciones CRUD sobre la entidad <see cref="Parameter"/>.
    /// </summary>
    [ApiController]
    [Route("api/Parameter")]
    public class ParameterController : ControllerBase
    {
        private readonly IParameterService _parameterService;

        /// <summary>
        /// Constructor que inyecta la capa de servicio para <see cref="Parameter"/>.
        /// </summary>
        /// <param name="parameterService">Servicio que contiene la lógica de negocio de <see cref="Parameter"/>.</param>
        public ParameterController(IParameterService parameterService)
        {
            _parameterService = parameterService ?? throw new ArgumentNullException(nameof(parameterService));
        }

        /// <summary>
        /// Crea un nuevo parámetro global.
        /// </summary>
        /// <param name="dto">Datos necesarios para crear el parámetro.</param>
        /// <returns>
        /// 201 Created si la creación es exitosa, o 400 BadRequest si alguno de los datos es inválido.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateParameterAsync([FromBody] CreateParameterDto dto)
        {
            var createdParameter = await _parameterService.CreateAsync(dto).FirstAsync();
            
            return CreatedAtAction("GetParameterById", new { id = createdParameter.ParameterId }, createdParameter);
        }

        /// <summary>
        /// Obtiene un parámetro por su identificador (ID).
        /// </summary>
        /// <param name="id">Identificador del parámetro a buscar.</param>
        /// <returns>El parámetro con el ID especificado.</returns>
        [HttpGet("getbyid/{id}", Name = "GetParameterById")]
        public async Task<IActionResult> GetParameterByIdAsync(int id)
        {
            var parameter = await _parameterService.GetByIdAsync(id).FirstAsync();
            if (parameter == null)
            {
                return NotFound();
            }
            return Ok(parameter);
        }

        /// <summary>
        /// Obtiene un parámetro por su nombre.
        /// </summary>
        /// <param name="name">Nombre del parámetro a buscar.</param>
        /// <returns>El parámetro con el nombre especificado.</returns>
        [HttpGet("getbyname/{name}")]
        public async Task<IActionResult> GetParameterByNameAsync(string name)
        {
            var parameter = await _parameterService.GetByNameAsync(name).FirstAsync();
            if (parameter == null)
            {
                return NotFound();
            }
            return Ok(parameter);
        }
    }
}