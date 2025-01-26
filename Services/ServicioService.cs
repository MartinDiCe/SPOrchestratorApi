using Microsoft.Extensions.Logging;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Exceptions;

namespace SPOrchestratorAPI.Services;

public class ServicioService
{
    private readonly IRepository<Servicio> _servicioRepository;
    private readonly ILogger<ServicioService> _logger;

    public ServicioService(IRepository<Servicio> servicioRepository, ILogger<ServicioService> logger)
    {
        _servicioRepository = servicioRepository;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Servicio>> GetAllAsync()
    {
        try
        {
            var servicios = await _servicioRepository.GetAllAsync();
            if (!servicios.Any())
            {
                throw new NotFoundException("No se encontraron servicios.");
            }
            return servicios;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los servicios.");
            throw new ServiceException("Ocurrió un error al recuperar los servicios.", ex);
        }
    }
    
    public async Task<Servicio?> GetByIdAsync(int id)
    {
        try
        {
            var servicio = await _servicioRepository.GetByIdAsync(id);
            if (servicio == null)
            {
                throw new NotFoundException($"No se encontró el servicio con ID {id}.");
            }
            return servicio;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener el servicio con ID {id}.");
            throw new ServiceException($"Ocurrió un error al recuperar el servicio con ID {id}.", ex);
        }
    }
    
    public async Task<Servicio> CreateAsync(Servicio servicio)
    {
        try
        {
            if (string.IsNullOrEmpty(servicio.Name))
                throw new ArgumentException("El nombre del servicio es obligatorio.");

            var createdServicio = await _servicioRepository.AddAsync(servicio);
            return createdServicio;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear un servicio.");
            throw new ServiceException("Ocurrió un error al crear el servicio.", ex);
        }
    }
    
    public async Task UpdateAsync(Servicio servicio)
    {
        try
        {
            var existingServicio = await _servicioRepository.GetByIdAsync(servicio.Id);
            if (existingServicio == null)
            {
                throw new NotFoundException($"No se encontró el servicio con ID {servicio.Id}.");
            }

            await _servicioRepository.UpdateAsync(servicio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar el servicio con ID {servicio.Id}.");
            throw new ServiceException($"Ocurrió un error al actualizar el servicio con ID {servicio.Id}.", ex);
        }
    }
    
    public async Task DeleteAsync(Servicio servicio)
    {
        try
        {
            var existingServicio = await _servicioRepository.GetByIdAsync(servicio.Id);
            if (existingServicio == null)
            {
                throw new NotFoundException($"No se encontró el servicio con ID {servicio.Id}.");
            }

            await _servicioRepository.DeleteAsync(servicio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar el servicio con ID {servicio.Id}.");
            throw new ServiceException($"Ocurrió un error al eliminar el servicio con ID {servicio.Id}.", ex);
        }
    }
}
