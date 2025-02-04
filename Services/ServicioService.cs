using System.Reactive; 
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.DTOs;
using SPOrchestratorAPI.Services.Logging;

namespace SPOrchestratorAPI.Services;

/// <summary>
/// Implementación del servicio para la gestión de servicios.
/// </summary>
public class ServicioService(
    ServicioRepository servicioRepository,
    ILoggerService<ServicioService> logger,
    IServiceExecutor serviceExecutor)
    : IServicioService
{
    private readonly ServicioRepository _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
    private readonly ILoggerService<ServicioService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceExecutor _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor)); // Corrección aquí

    /// <inheritdoc />
    public IObservable<IEnumerable<Servicio>> GetAllAsync()
    {
        return _servicioRepository.GetAllAsync(s => !s.Deleted)
            .Select(servicios =>
            {
                var servicioList = servicios.ToList();
                if (!servicioList.Any())
                {
                    throw new NotFoundException("No se encontraron servicios.");
                }
                return servicioList;
            })
            .Catch<IEnumerable<Servicio>, Exception>(ex =>
            {
                _logger.LogError($"Error en GetAllAsync: {ex.Message}", ex);
                return Observable.Throw<IEnumerable<Servicio>>(ex);
            });
    }

    /// <inheritdoc />
    public IObservable<Servicio> GetByIdAsync(int id)
    {
        return _servicioRepository.GetByIdAsync(id)
            .Select(servicio => servicio ?? throw new NotFoundException($"No se encontró el servicio con ID {id}."))
            .Where(servicio => !servicio.Deleted)
            .Catch<Servicio, Exception>(ex =>
            {
                _logger.LogError($"Error en GetByIdAsync: {ex.Message}", ex);
                return Observable.Throw<Servicio>(ex);
            });
    }

    /// <inheritdoc />
    public IObservable<Servicio> CreateAsync(CreateServicioDto servicioDto)
    {
        return Observable.FromAsync(async () =>
            {
                _logger.LogInfo("Iniciando la creación del servicio...");

                // Validación del nombre del servicio
                if (string.IsNullOrEmpty(servicioDto.Name))
                {
                    _logger.LogWarning("El nombre del servicio es obligatorio.");
                    throw new ArgumentException("El nombre del servicio es obligatorio.");
                }

                _logger.LogInfo($"Verificando si ya existe un servicio con el nombre: {servicioDto.Name}");

                // Verifica si ya existe un servicio con el mismo nombre usando GetByNameAsync
                var existingService = await _servicioRepository.GetByNameAsync(servicioDto.Name);

                // Si el servicio ya existe, lanzamos una excepción
                if (existingService != null)
                {
                    _logger.LogWarning($"Ya existe un servicio con el nombre '{servicioDto.Name}'.");
                    throw new InvalidOperationException($"Ya existe un servicio con el nombre '{servicioDto.Name}'.");
                }

                // Si el servicio no existe, creamos un nuevo servicio
                var servicio = new Servicio
                {
                    Name = servicioDto.Name,
                    Description = servicioDto.Description,
                    Status = servicioDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _logger.LogInfo($"Servicio creado: {servicio.Name}.");

                // Agregar el nuevo servicio a la base de datos
                var addedService = await _servicioRepository.AddAsync(servicio);

                _logger.LogInfo($"Servicio {addedService.Name} creado correctamente con ID {addedService.Id}.");

                return addedService;
            })
            .Catch<Servicio, Exception>(ex =>
            {
                _logger.LogError($"Error en CreateAsync: {ex.Message}", ex);
                return Observable.Throw<Servicio>(ex);
            });
    }

    /// <inheritdoc />
    public IObservable<Unit> UpdateAsync(UpdateServicioDto servicioDto)
    {
        return GetByIdAsync(servicioDto.Id)
                .SelectMany(servicio =>
                {
                    servicio.Name = servicioDto.Name;
                    servicio.Description = servicioDto.Description;
                    servicio.Status = servicioDto.Status;
                    servicio.UpdatedAt = DateTime.UtcNow;
                    servicio.UpdatedBy = "System";
                    return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
                })
            .Catch<Unit, Exception>(ex =>
            {
                _logger.LogError($"Error en UpdateAsync: {ex.Message}", ex);
                return Observable.Throw<Unit>(ex);
            });
    }

    /// <inheritdoc />
public IObservable<Unit> ChangeStatusAsync(int id, bool newStatus)
{
    return GetByIdAsync(id)
        .SelectMany(servicio =>
        {
            servicio.Status = newStatus;
            servicio.UpdatedAt = DateTime.UtcNow;
            servicio.UpdatedBy = "System";

            _logger.LogInfo($"Cambio de estado para el servicio con ID {id} a {newStatus}.");
            
            return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
        })
        .Catch<Unit, Exception>(ex =>
        {
            _logger.LogError($"Error al cambiar el estado del servicio con ID {id}: {ex.Message}", ex);
            return Observable.Throw<Unit>(ex);
        });
}

/// <inheritdoc />
public IObservable<Unit> DeleteBySystemAsync(int id)
{
    return GetByIdAsync(id)
        .SelectMany(servicio =>
        {
            servicio.Deleted = true;
            servicio.DeletedAt = DateTime.UtcNow;
            servicio.DeletedBy = "System";

            _logger.LogInfo($"Marcando el servicio con ID {id} como eliminado.");
            
            return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
        })
        .Catch<Unit, Exception>(ex =>
        {
            _logger.LogError($"Error al eliminar el servicio con ID {id}: {ex.Message}", ex);
            return Observable.Throw<Unit>(ex);
        });
}

/// <inheritdoc />
public IObservable<Unit> RestoreBySystemAsync(int id)
{
    return _servicioRepository.GetByIdAsync(id)
        .Select(servicio => servicio ?? throw new NotFoundException($"No se encontró un servicio eliminado con ID {id}."))
        .Where(servicio => servicio.Deleted)
        .SelectMany(servicio =>
        {
            servicio.Deleted = false;
            servicio.DeletedAt = null;
            servicio.DeletedBy = null;
            servicio.UpdatedAt = DateTime.UtcNow;
            servicio.UpdatedBy = "System";

            _logger.LogInfo($"Restaurando el servicio con ID {id}.");

            return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
        })
        .Catch<Unit, Exception>(ex =>
        {
            _logger.LogError($"Error al restaurar el servicio con ID {id}: {ex.Message}", ex);
            return Observable.Throw<Unit>(ex);
        });
    }

    /// <inheritdoc />
    public IObservable<Servicio> GetByNameAsync(string name)
    {
        return _servicioRepository.GetByNameAsync(name)
            .Select(servicio => servicio ?? throw new NotFoundException($"No se encontró el servicio con el nombre {name}."))
            .Catch<Servicio, Exception>(ex =>
            {
                _logger.LogError($"Error al obtener el servicio con nombre {name}: {ex.Message}", ex);
                return Observable.Throw<Servicio>(ex);
            });
    }
    
}
