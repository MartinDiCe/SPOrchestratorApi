using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services;

namespace SPOrchestratorAPI.Controllers;

/// <summary>
/// Controlador para gestionar los servicios.
/// </summary>
[Route("api/servicio")]
[ApiController]
public class ServicioController : ControllerBase
{
    private readonly ServicioService _servicioService;

    public ServicioController(ServicioService servicioService)
    {
        _servicioService = servicioService;
    }

    /// <summary>
    /// Obtiene todos los servicios registrados (excluyendo los eliminados).
    /// </summary>
    [HttpGet("getall")]
    [ActionName("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _servicioService.GetAllAsync());
    }

    /// <summary>
    /// Obtiene un servicio por su ID.
    /// </summary>
    [HttpGet("getbyid/{id}")]
    [ActionName("GetById")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await _servicioService.GetByIdAsync(id));
    }

    /// <summary>
    /// Obtiene un servicio por su nombre.
    /// </summary>
    [HttpGet("getbyname/{name}")]
    [ActionName("GetByName")]
    public async Task<IActionResult> GetByName(string name)
    {
        return Ok(await _servicioService.GetByNameAsync(name));
    }

    /// <summary>
    /// Crea un nuevo servicio.
    /// </summary>
    [HttpPost("create")]
    [ActionName("Create")]
    public async Task<IActionResult> Create([FromBody] Servicio servicio)
    {
        var createdService = await _servicioService.CreateAsync(servicio);
        return CreatedAtAction(nameof(GetById), new { id = createdService.Id }, createdService);
    }

    /// <summary>
    /// Actualiza un servicio existente.
    /// </summary>
    [HttpPut("update/{id}")]
    [ActionName("Update")]
    public async Task<IActionResult> Update(int id, [FromBody] Servicio servicio)
    {
        if (id != servicio.Id)
        {
            return BadRequest(new { mensaje = "El ID en la URL no coincide con el ID del servicio." });
        }

        await _servicioService.UpdateAsync(servicio);
        return NoContent();
    }

    /// <summary>
    /// Cambia el estado de un servicio (activo/inactivo).
    /// </summary>
    [HttpPatch("changestatus/{id}/{status}")]
    [ActionName("ChangeStatus")]
    public async Task<IActionResult> ChangeStatus(int id, bool status)
    {
        await _servicioService.ChangeStatusAsync(id, status);
        return NoContent();
    }

    /// <summary>
    /// Marca un servicio como eliminado (eliminación lógica).
    /// </summary>
    [HttpDelete("delete/{id}")]
    [ActionName("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _servicioService.DeleteBySystemAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Restaura un servicio eliminado.
    /// </summary>
    [HttpPost("restore/{id}")]
    [ActionName("Restore")]
    public async Task<IActionResult> Restore(int id)
    {
        await _servicioService.RestoreBySystemAsync(id);
        return NoContent();
    }
}
