using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services;

namespace SPOrchestratorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicioController : ControllerBase
{
    private readonly ServicioService _servicioService;

    public ServicioController(ServicioService servicioService)
    {
        _servicioService = servicioService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _servicioService.GetAllAsync();
        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var servicio = await _servicioService.GetByIdAsync(id);
            return Ok(servicio);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ServiceException ex)
        {
            return StatusCode(500, new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Servicio servicio)
    {
        var createdService = await _servicioService.CreateAsync(servicio);
        return CreatedAtAction(nameof(GetById), new { id = createdService.Id }, createdService);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Servicio servicio)
    {
        if (id != servicio.Id) return BadRequest();
        await _servicioService.UpdateAsync(servicio);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _servicioService.GetByIdAsync(id);
        if (service == null) return NotFound();
        await _servicioService.DeleteAsync(service);
        return NoContent();
    }
}