using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services;
using System.Reactive.Linq;

namespace SPOrchestratorAPI.Controllers;

/// <summary>
/// Controlador para gestionar la configuración de los servicios.
/// </summary>
[Route("api/servicioconfiguracion")]
[ApiController]
public class ServicioConfiguracionController : ControllerBase
{
    private readonly ServicioConfiguracionService _servicioConfiguracionService;

    public ServicioConfiguracionController(ServicioConfiguracionService servicioConfiguracionService)
    {
        _servicioConfiguracionService = servicioConfiguracionService;
    }

    /// <summary>
    /// Obtiene todas las configuraciones de servicios activas.
    /// </summary>
    [HttpGet("getall")]
    public IObservable<IActionResult> GetAll()
    {
        return _servicioConfiguracionService.GetAllAsync()
            .Select(configs => Ok(configs) as IActionResult);
    }

    /// <summary>
    /// Obtiene la configuración de un servicio por su ID.
    /// </summary>
    [HttpGet("getbyservicioid/{servicioId}")]
    public IObservable<IActionResult> GetByServicioId(int servicioId)
    {
        return _servicioConfiguracionService.GetByServicioIdAsync(servicioId)
            .Select(config => Ok(config) as IActionResult);
    }

    /// <summary>
    /// Crea una nueva configuración de servicio.
    /// </summary>
    [HttpPost("create")]
    public IObservable<IActionResult> Create([FromBody] ServicioConfiguracion config)
    {
        return _servicioConfiguracionService.CreateAsync(config)
            .Select(createdConfig => CreatedAtAction(nameof(GetByServicioId), new { servicioId = createdConfig.ServicioId }, createdConfig) as IActionResult);
    }

    /// <summary>
    /// Actualiza una configuración existente.
    /// </summary>
    [HttpPut("update/{id}")]
    public IObservable<IActionResult> Update(int id, [FromBody] ServicioConfiguracion config)
    {
        if (id != config.Id)
        {
            return Observable.Return(BadRequest(new { mensaje = "El ID en la URL no coincide con el ID de la configuración." }) as IActionResult);
        }

        return _servicioConfiguracionService.UpdateAsync(config)
            .Select(_ => NoContent() as IActionResult);
    }

    /// <summary>
    /// Elimina una configuración de servicio (eliminación lógica).
    /// </summary>
    [HttpDelete("delete/{id}")]
    public IObservable<IActionResult> Delete(int id)
    {
        return _servicioConfiguracionService.DeleteBySystemAsync(id)
            .Select(_ => NoContent() as IActionResult);
    }
}
