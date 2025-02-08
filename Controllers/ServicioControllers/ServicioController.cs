using System.Reactive.Linq;
using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.DTOs.ServicioDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ServicioServices;

namespace SPOrchestratorAPI.Controllers.ServicioControllers;

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
        
    /// <summary>
    /// Retorna todos los servicios no eliminados (pueden estar activos o inactivos).
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllServicesAsync()
    {
        var services = await _servicioService
            .GetAllAsync()
            .FirstAsync();

        return Ok(services);
    }

    /// <summary>
    /// Retorna todos los servicios activos (Status = true, Deleted = false).
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveServicesAsync()
    {
        var services = await _servicioService
            .GetActiveServicesAsync()
            .FirstAsync();

        return Ok(services);
    }

    /// <summary>
    /// Elimina lógicamente un servicio (soft delete).
    /// </summary>
    [HttpPut("softdelete/{id}")]
    public async Task<IActionResult> SoftDeleteServiceAsync(int id)
    {
        var deleted = await _servicioService
            .SoftDeleteAsync(id)
            .FirstAsync();

        return Ok(deleted); 
    }

    /// <summary>
    /// Restaura un servicio previamente marcado como eliminado.
    /// </summary>
    [HttpPut("restore/{id}")]
    public async Task<IActionResult> RestoreServiceAsync(int id)
    {
        var restored = await _servicioService
            .RestoreAsync(id)
            .FirstAsync();

        return Ok(restored);
    }

    /// <summary>
    /// Inactiva un servicio, estableciendo <c>Status = false</c>.
    /// </summary>
    [HttpPut("desactivate/{id}")]
    public async Task<IActionResult> DeactivateServiceAsync(int id)
    {
        var deactivated = await _servicioService
            .DeactivateAsync(id)
            .FirstAsync();

        return Ok(deactivated);
    }

    /// <summary>
    /// Activa un servicio, estableciendo <c>Status = true</c>.
    /// </summary>
    [HttpPut("activate/{id}")]
    public async Task<IActionResult> ActivateServiceAsync(int id)
    {
        var activated = await _servicioService
            .ActivateAsync(id)
            .FirstAsync();

        return Ok(activated);
    }

    /// <summary>
    /// Actualiza un servicio existente.
    /// </summary>
    [HttpPut("update")]
    public async Task<IActionResult> UpdateServiceAsync([FromBody] UpdateServicioDto dto)
    {
        var updated = await _servicioService
            .UpdateAsync(dto)
            .FirstAsync();
            
        return Ok(updated);
    }
}