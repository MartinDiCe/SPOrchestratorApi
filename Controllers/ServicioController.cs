using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Services;
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.DTOs;

namespace SPOrchestratorAPI.Controllers;

/// <summary>
/// Controlador para gestionar los servicios de manera reactiva.
/// </summary>
[Route("api/servicio")]
[ApiController]
public class ServicioController(IServicioService servicioService) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los servicios registrados (excluyendo los eliminados).
    /// </summary>
    [HttpGet("getall")]
    public IObservable<IActionResult> GetAll()
    {
        return servicioService.GetAllAsync()
            .Select(servicios => Ok(servicios) as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Obtiene un servicio por su ID.
    /// </summary>
    [HttpGet("getbyid/{id}")]
    public IObservable<IActionResult> GetById(int id)
    {
        return servicioService.GetByIdAsync(id)
            .Select(servicio => Ok(servicio) as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Obtiene un servicio por su nombre.
    /// </summary>
    [HttpGet("getbyname/{name}")]
    public IObservable<IActionResult> GetByName(string name)
    {
        return servicioService.GetByNameAsync(name)
            .Select(servicio => Ok(servicio) as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Crea un nuevo servicio.
    /// </summary>
    [HttpPost("create")]
    public IObservable<IActionResult> Create([FromBody] CreateServicioDto createServicioDto)
    {
        return servicioService.CreateAsync(createServicioDto)
            .Select(createdService => CreatedAtAction(nameof(GetById), new { id = createdService.Id }, createdService) as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Actualiza un servicio existente.
    /// </summary>
    [HttpPut("update/{id}")]
    public IObservable<IActionResult> Update(int id, [FromBody] UpdateServicioDto servicio)
    {
        if (id != servicio.Id)
        {
            return Observable.Throw<IActionResult>(new ArgumentException("El ID en la URL no coincide con el ID del servicio."));
        }

        return servicioService.UpdateAsync(servicio)
            .Select(_ => NoContent() as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Cambia el estado de un servicio (activo/inactivo).
    /// </summary>
    [HttpPatch("changestatus/{id}/{status}")]
    public IObservable<IActionResult> ChangeStatus(int id, bool status)
    {
        return servicioService.ChangeStatusAsync(id, status)
            .Select(_ => NoContent() as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Marca un servicio como eliminado (eliminación lógica).
    /// </summary>
    [HttpDelete("delete/{id}")]
    public IObservable<IActionResult> Delete(int id)
    {
        return servicioService.DeleteBySystemAsync(id)
            .Select(_ => NoContent() as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }

    /// <summary>
    /// Restaura un servicio eliminado.
    /// </summary>
    [HttpPost("restore/{id}")]
    public IObservable<IActionResult> Restore(int id)
    {
        return servicioService.RestoreBySystemAsync(id)
            .Select(_ => NoContent() as IActionResult)
            .Catch<IActionResult, Exception>(ex => Observable.Return(StatusCode(500, new { mensaje = ex.Message }) as IActionResult));
    }
}
