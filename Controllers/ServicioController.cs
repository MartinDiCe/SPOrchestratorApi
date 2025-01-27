using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace SPOrchestratorAPI.Controllers;

/// <summary>
/// Controlador para gestionar los servicios de manera reactiva.
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
    public IObservable<IActionResult> GetAll()
    {
        return _servicioService.GetAllAsync()
            .Select(servicios => Ok(servicios) as IActionResult);
    }

    /// <summary>
    /// Obtiene un servicio por su ID.
    /// </summary>
    [HttpGet("getbyid/{id}")]
    public IObservable<IActionResult> GetById(int id)
    {
        return _servicioService.GetByIdAsync(id)
            .Select(servicio => Ok(servicio) as IActionResult);
    }

    /// <summary>
    /// Obtiene un servicio por su nombre.
    /// </summary>
    [HttpGet("getbyname/{name}")]
    public IObservable<IActionResult> GetByName(string name)
    {
        return _servicioService.GetByNameAsync(name)
            .Select(servicio => Ok(servicio) as IActionResult);
    }

    /// <summary>
    /// Crea un nuevo servicio.
    /// </summary>
    [HttpPost("create")]
    public IObservable<IActionResult> Create([FromBody] Servicio servicio)
    {
        return _servicioService.CreateAsync(servicio)
            .Select(createdService => CreatedAtAction(nameof(GetById), new { id = createdService.Id }, createdService) as IActionResult);
    }

    /// <summary>
    /// Actualiza un servicio existente.
    /// </summary>
    [HttpPut("update/{id}")]
    public IObservable<IActionResult> Update(int id, [FromBody] Servicio servicio)
    {
        if (id != servicio.Id)
        {
            return Observable.Return(BadRequest(new { mensaje = "El ID en la URL no coincide con el ID del servicio." }) as IActionResult);
        }

        return _servicioService.UpdateAsync(servicio)
            .Select(_ => NoContent() as IActionResult);
    }

    /// <summary>
    /// Cambia el estado de un servicio (activo/inactivo).
    /// </summary>
    [HttpPatch("changestatus/{id}/{status}")]
    public IObservable<IActionResult> ChangeStatus(int id, bool status)
    {
        return _servicioService.ChangeStatusAsync(id, status)
            .Select(_ => NoContent() as IActionResult);
    }

    /// <summary>
    /// Marca un servicio como eliminado (eliminación lógica).
    /// </summary>
    [HttpDelete("delete/{id}")]
    public IObservable<IActionResult> Delete(int id)
    {
        return _servicioService.DeleteBySystemAsync(id)
            .Select(_ => NoContent() as IActionResult);
    }

    /// <summary>
    /// Restaura un servicio eliminado.
    /// </summary>
    [HttpPost("restore/{id}")]
    public IObservable<IActionResult> Restore(int id)
    {
        return _servicioService.RestoreBySystemAsync(id)
            .Select(_ => NoContent() as IActionResult);
    }
}
