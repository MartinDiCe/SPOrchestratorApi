using System.Reactive.Linq;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.DTOs.ServicioDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories.ServicioRepositories;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Services.ServicioServices;

/// <summary>
/// Implementación del servicio para la gestión de <see cref="Servicio"/>.
/// </summary>
public class ServicioService : IServicioService
{
    private readonly IServicioRepository _servicioRepository;
    private readonly ILoggerService<ServicioService> _logger;
    private readonly IServiceExecutor _serviceExecutor;
    private IServicioService _servicioServiceImplementation;

    /// <summary>
    /// Constructor de la clase <see cref="ServicioService"/>.
    /// </summary>
    /// <param name="servicioRepository">El repositorio para acceder a datos de <see cref="Servicio"/>.</param>
    /// <param name="logger">Servicio de logging.</param>
    /// <param name="serviceExecutor">Ejecutor reactivo para manejar errores y suscripciones.</param>
    /// <exception cref="ArgumentNullException">Si algún parámetro es nulo.</exception>
    public ServicioService(
        IServicioRepository servicioRepository,
        ILoggerService<ServicioService> logger,
        IServiceExecutor serviceExecutor)
    {
        _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
    }

    /// <inheritdoc />
    public IObservable<Servicio> CreateAsync(CreateServicioDto servicioDto)
    {
        // Ejecutamos la lógica de negocio dentro del serviceExecutor
        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo("Iniciando la creación del servicio...");

            // 1) Validaciones de entrada
            if (string.IsNullOrWhiteSpace(servicioDto.Name))
            {
                _logger.LogWarning("El nombre del servicio es obligatorio.");
                throw new ArgumentException("El nombre del servicio es obligatorio.", nameof(servicioDto.Name));
            }

            _logger.LogInfo($"Verificando si ya existe un servicio con el nombre: {servicioDto.Name}");
            
            return _servicioRepository
                .GetByNameAsync(servicioDto.Name)
                .Catch<Servicio, ResourceNotFoundException>(_ =>
                {
                    _logger.LogInfo($"No existe un servicio con el nombre {servicioDto.Name}, se procede a crear.");
                    return Observable.Return<Servicio>(null);
                })
                .Select(existingService =>
                {
                    if (existingService != null)
                    {

                        _logger.LogWarning($"Ya existe un servicio con el nombre '{servicioDto.Name}'.");
                        throw new InvalidOperationException($"Ya existe un servicio con el nombre '{servicioDto.Name}'.");
                    }

                    var servicio = new Servicio
                    {
                        Name = servicioDto.Name,
                        Description = servicioDto.Description,
                        Status = servicioDto.Status,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System" 
                    };
                    
                    return _servicioRepository.AddAsync(servicio);
                })

                .Switch()

                .Do(createdService =>
                {
                    _logger.LogInfo($"Servicio '{createdService.Name}' creado correctamente con ID {createdService.Id}.");
                });
        });
    }

    /// <inheritdoc />
    public IObservable<Servicio> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor que 0.", nameof(id));
        }
        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo($"Buscando servicio con ID {id} (excluyendo registros eliminados).");
                
            return _servicioRepository.GetByIdAsync(id);
        });
    }

    /// <inheritdoc />
    public IObservable<Servicio> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre no puede ser vacío.", nameof(name));
        }

        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo($"Buscando servicio con el nombre '{name}' (excluyendo registros eliminados).");
            return _servicioRepository.GetByNameAsync(name);
        });
    }
        
    /// <inheritdoc />
    public IObservable<IList<Servicio>> GetAllAsync()
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo("Obteniendo TODOS los servicios no eliminados (aunque podrían estar inactivos).");
            return _servicioRepository.GetAllAsync();
        });
    }
        
    /// <inheritdoc />
    public IObservable<IList<Servicio>> GetActiveServicesAsync()
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo("Obteniendo servicios activos (Status=true, !Deleted).");
            return _servicioRepository.GetActiveServicesAsync();
        });
    }
        
    /// <inheritdoc />
    public IObservable<Servicio> SoftDeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor que 0 para eliminar.", nameof(id));
        }

        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo($"Solicitando eliminación lógica del servicio con ID {id}.");
            return _servicioRepository
                .SoftDeleteAsync(id)
                .Do(deleted => 
                {
                    _logger.LogInfo($"Servicio con ID {deleted.Id} marcado como eliminado (soft delete).");
                });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Servicio> RestoreAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor que 0 para restaurar.", nameof(id));
        }

        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo($"Restaurando el servicio con ID {id}.");
            return _servicioRepository
                .RestoreAsync(id)
                .Do(restored =>
                {
                    _logger.LogInfo($"Servicio con ID {restored.Id} restaurado correctamente (Deleted=false).");
                });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Servicio> DeactivateAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor que 0 para inactivarlo.", nameof(id));
        }

        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo($"Inactivando el servicio con ID {id}.");
            return _servicioRepository
                .DeactivateAsync(id)
                .Do(deactivated =>
                {
                    _logger.LogInfo($"Servicio con ID {deactivated.Id} inactivado (Status=false).");
                });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Servicio> ActivateAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor que 0 para activarlo.", nameof(id));
        }

        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo($"Activando el servicio con ID {id}.");
            return _servicioRepository
                .ActivateAsync(id)
                .Do(activated =>
                {
                    _logger.LogInfo($"Servicio con ID {activated.Id} activado (Status=true).");
                });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Servicio> UpdateAsync(UpdateServicioDto dto)
    {
        if (dto.Id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor que 0 para actualizar.", nameof(dto.Id));
        }
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("El nombre del servicio es obligatorio.", nameof(dto.Name));
        }

        return _serviceExecutor.ExecuteAsync(() =>
        {
            _logger.LogInfo($"Actualizando el servicio con ID {dto.Id}.");
                
            var servicioToUpdate = new Servicio
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status
            };
                
            return _servicioRepository.UpdateAsync(servicioToUpdate)
                .Do(updated =>
                {
                    _logger.LogInfo($"Servicio con ID {updated.Id} se actualizó correctamente en la BD.");
                });
        });
    }
}