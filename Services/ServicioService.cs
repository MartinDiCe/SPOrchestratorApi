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

    /// <summary>
    /// Obtiene todos los servicios activos (excluyendo eliminados).
    /// </summary>
    public async Task<IEnumerable<Servicio>> GetAllAsync()
    {
        var servicios = await _servicioRepository.GetAllAsync(s => !s.Deleted);
        if (!servicios.Any())
        {
            throw new NotFoundException("No se encontraron servicios.");
        }
        return servicios;
    }

    /// <summary>
    /// Obtiene un servicio por su ID, solo si no está eliminado.
    /// </summary>
    public async Task<Servicio?> GetByIdAsync(int id)
    {
        var servicio = await _servicioRepository.GetByIdAsync(id);
        if (servicio == null || servicio.Deleted)
        {
            throw new NotFoundException($"No se encontró el servicio con ID {id}.");
        }
        return servicio;
    }

    /// <summary>
    /// Obtiene un servicio por su nombre, solo si no está eliminado.
    /// </summary>
    public async Task<Servicio?> GetByNameAsync(string name)
    {
        var servicio = await _servicioRepository.GetAllAsync(s => s.Name == name && !s.Deleted);
        return servicio.FirstOrDefault();
    }

    /// <summary>
    /// Crea un nuevo servicio.
    /// </summary>
    public async Task<Servicio> CreateAsync(Servicio servicio)
    {
        if (string.IsNullOrEmpty(servicio.Name))
        {
            throw new ArgumentException("El nombre del servicio es obligatorio.");
        }

        return await _servicioRepository.AddAsync(servicio);
    }

    /// <summary>
    /// Actualiza un servicio existente.
    /// </summary>
    public async Task UpdateAsync(Servicio servicio)
    {
        var existingServicio = await GetByIdAsync(servicio.Id);
        if (existingServicio == null)
        {
            throw new NotFoundException($"No se encontró el servicio con ID {servicio.Id}.");
        }

        servicio.UpdatedAt = DateTime.UtcNow;
        servicio.UpdatedBy = "System";
        await _servicioRepository.UpdateAsync(servicio);
    }

    /// <summary>
    /// Cambia el estado de un servicio (activo/inactivo).
    /// </summary>
    public async Task ChangeStatusAsync(int id, bool newStatus)
    {
        var servicio = await GetByIdAsync(id);
        if (servicio == null)
        {
            throw new NotFoundException($"No se encontró el servicio con ID {id}.");
        }

        servicio.Status = newStatus;
        servicio.UpdatedAt = DateTime.UtcNow;
        servicio.UpdatedBy = "System";
        await _servicioRepository.UpdateAsync(servicio);
    }

    /// <summary>
    /// Marca un servicio como eliminado (eliminación lógica).
    /// </summary>
    public async Task DeleteBySystemAsync(int id)
    {
        var servicio = await GetByIdAsync(id);
        if (servicio == null)
        {
            throw new NotFoundException($"No se encontró el servicio con ID {id}.");
        }

        servicio.Deleted = true;
        servicio.DeletedAt = DateTime.UtcNow;
        servicio.DeletedBy = "System";
        await _servicioRepository.UpdateAsync(servicio);
    }

    /// <summary>
    /// Restaura un servicio eliminado.
    /// </summary>
    public async Task RestoreBySystemAsync(int id)
    {
        var servicio = await _servicioRepository.GetByIdAsync(id);
        if (servicio == null || !servicio.Deleted)
        {
            throw new NotFoundException($"No se encontró un servicio eliminado con ID {id}.");
        }

        servicio.Deleted = false;
        servicio.DeletedAt = null;
        servicio.DeletedBy = null;
        servicio.UpdatedAt = DateTime.UtcNow;
        servicio.UpdatedBy = "System";
        await _servicioRepository.UpdateAsync(servicio);
    }
}
