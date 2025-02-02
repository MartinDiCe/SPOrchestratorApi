using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.DTOs;
using SPOrchestratorAPI.Services;
using System.Reactive.Linq;

namespace SPOrchestratorAPI.Controllers;

/// <summary>
/// Controlador para gestionar la configuración de los servicios.
/// </summary>
[Route("api/servicioconfiguracion")]
[ApiController]
public class ServicioConfiguracionController(IServicioConfiguracionService servicioConfiguracionService)
    : ControllerBase
{
    /// <summary>
    /// Obtiene todas las configuraciones de servicios activas.
    /// </summary>
    [HttpGet("getall")]
    public IObservable<IActionResult> GetAll()
    {
        return servicioConfiguracionService.GetAllAsync()
            .Select(configs => Ok(configs) as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Obtiene la configuración de un servicio por su ID.
    /// </summary>
    [HttpGet("getbyservicioid/{servicioId}")]
    public IObservable<IActionResult> GetByServicioId(int servicioId)
    {
        return servicioConfiguracionService.GetByServicioIdAsync(servicioId)
            .Select(config => Ok(config) as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(404, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Crea una nueva configuración de servicio.
    /// </summary>
    [HttpPost("create")]
    public IObservable<IActionResult> Create([FromBody] CreateServicioConfiguracionDto configDto)
    {
        return servicioConfiguracionService.CreateAsync(configDto)
            .Select(createdConfig => CreatedAtAction(nameof(GetByServicioId), new { servicioId = createdConfig.ServicioId }, createdConfig) as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Actualiza una configuración existente.
    /// </summary>
    [HttpPut("update/{id}")]
    public IObservable<IActionResult> Update(int id, [FromBody] UpdateServicioConfiguracionDto configDto)
    {
        if (id != configDto.ServicioId)
        {
            return Observable.Return(BadRequest(new { mensaje = "El ID en la URL no coincide con el ID de la configuración." }) as IActionResult);
        }

        return servicioConfiguracionService.UpdateAsync(configDto)
            .Select(_ => NoContent() as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Elimina una configuración de servicio (eliminación lógica).
    /// </summary>
    [HttpDelete("delete/{id}")]
    public IObservable<IActionResult> Delete(int id)
    {
        return servicioConfiguracionService.DeleteBySystemAsync(id)
            .Select(_ => NoContent() as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Restaura una configuración eliminada.
    /// </summary>
    [HttpPost("restore/{id}")]
    public IObservable<IActionResult> Restore(int id)
    {
        return servicioConfiguracionService.RestoreBySystemAsync(id)
            .Select(_ => NoContent() as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }
}
