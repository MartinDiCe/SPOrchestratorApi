using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Models.Repositories.ServicioRepositories;

/// <summary>
/// Implementación de <see cref="IServicioRepository"/> 
/// para el acceso a datos de la entidad <see cref="Servicio"/> de manera reactiva.
/// </summary>
public class ServicioRepository : IServicioRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILoggerService<ServicioRepository> _logger;
    private readonly DbSet<Entities.Servicio> _dbSet;
    private readonly IServiceExecutor  _serviceExecutor;
    private IServicioRepository _servicioRepositoryImplementation;

    /// <summary>
    /// Constructor de la clase <see cref="ServicioRepository"/>.
    /// </summary>
    /// <param name="context">El contexto de base de datos.</param>
    /// <param name="logger">Servicio de logging.</param>
    /// <param name="serviceExecutor">Ejecutor reactivo para manejar errores y suscripciones.</param>
    /// <exception cref="ArgumentNullException">Lanzada si alguno de los parámetros es nulo.</exception>
    public ServicioRepository(
        ApplicationDbContext context,
        ILoggerService<ServicioRepository> logger,
        IServiceExecutor serviceExecutor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = _context.Set<Entities.Servicio>();
        _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
    }

    /// <inheritdoc />
    public IObservable<IList<Entities.Servicio>> GetActiveServicesAsync()
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo("Consultando servicios activos en la base de datos...");
                var services = await _dbSet
                    .Where(s => s.Status && !s.Deleted)
                    .ToListAsync();

                _logger.LogInfo($"Se obtuvieron {services.Count} servicios activos.");
                return services;
            });
        });
    }
        
    /// <inheritdoc />
    public IObservable<IList<Entities.Servicio>> GetAllAsync()
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo("Consultando todos los servicios no eliminados (independientemente de su Status)...");
                var services = await _dbSet
                    .Where(s => !s.Deleted) 
                    .ToListAsync();

                _logger.LogInfo($"Se obtuvieron {services.Count} servicios totales (no eliminados).");
                return services;
            });
        });
    }

    /// <inheritdoc />
    public IObservable<Entities.Servicio> GetByNameAsync(string name)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                var service = await _dbSet
                    .Where(s => s.Name == name && !s.Deleted)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    throw new ResourceNotFoundException(
                        $"No se encontró un servicio con el nombre '{name}'.");
                }
                return service;
            });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Entities.Servicio> GetByIdAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                var service = await _dbSet
                    .Where(s => s.Id == id && !s.Deleted)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    throw new ResourceNotFoundException(
                        $"No se encontró un servicio con ID {id}.");
                }
                return service;
            });
        });
    }

    /// <inheritdoc />
    public IObservable<Entities.Servicio> AddAsync(Entities.Servicio servicio)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Agregando el servicio '{servicio.Name}' a la base de datos...");

                _dbSet.Add(servicio);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Servicio '{servicio.Name}' persistido correctamente con ID {servicio.Id}.");
                return servicio;
            });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Entities.Servicio> SoftDeleteAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Marcando como eliminado (soft delete) el servicio con ID {id}...");
                var service = await _dbSet.FindAsync(id);

                if (service == null)
                {
                    _logger.LogWarning($"No se encontró un servicio con ID {id} para eliminarlo.");
                    throw new ResourceNotFoundException($"No se encontró un servicio con ID {id}.");
                }

                service.Deleted = true;
                service.DeletedAt = DateTime.UtcNow;
                service.DeletedBy = "System";
                _dbSet.Update(service);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Servicio con ID {id} marcado como eliminado.");
                return service;
            });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Entities.Servicio> RestoreAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Restaurando el servicio con ID {id} (soft delete)...");
                var service = await _dbSet.FindAsync(id);

                if (service == null)
                {
                    _logger.LogWarning($"No se encontró el servicio con ID {id} para restaurarlo.");
                    throw new ResourceNotFoundException($"No se encontró un servicio con ID {id}.");
                }

                service.Deleted = false;
                service.DeletedAt = null;
                service.DeletedBy = null;
                _dbSet.Update(service);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Servicio con ID {id} restaurado (Deleted = false).");
                return service;
            });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Entities.Servicio> DeactivateAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Inactivando el servicio con ID {id}...");
                var service = await _dbSet.FindAsync(id);

                if (service == null)
                {
                    _logger.LogWarning($"No se encontró un servicio con ID {id} para inactivarlo.");
                    throw new ResourceNotFoundException($"No se encontró un servicio con ID {id}.");
                }

                service.Status = false;
                _dbSet.Update(service);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Servicio con ID {id} inactivado (Status = false).");
                return service;
            });
        });
    }
        
    /// <inheritdoc />
    public IObservable<Entities.Servicio> ActivateAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Activando el servicio con ID {id}...");
                var service = await _dbSet.FindAsync(id);

                if (service == null)
                {
                    _logger.LogWarning($"No se encontró un servicio con ID {id} para activarlo.");
                    throw new ResourceNotFoundException($"No se encontró un servicio con ID {id}.");
                }

                service.Status = true;
                _dbSet.Update(service);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Servicio con ID {id} activado (Status = true).");
                return service;
            });
        });
    }

    /// <inheritdoc />
    public IObservable<Entities.Servicio> UpdateAsync(Entities.Servicio servicio)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                if (servicio.Id <= 0)
                {
                    throw new ArgumentException("El Id del servicio debe ser mayor que 0 para actualizar.", nameof(servicio.Id));
                }
                    
                _logger.LogInfo($"Actualizando el servicio con ID {servicio.Id}...");

                var existing = await _dbSet
                    .Where(s => s.Id == servicio.Id && !s.Deleted)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    _logger.LogWarning($"No se encontró un servicio con ID {servicio.Id} para actualizarlo.");
                    throw new ResourceNotFoundException($"No se encontró un servicio con ID {servicio.Id}.");
                }
                    
                existing.Name = servicio.Name;
                existing.Description = servicio.Description;
                existing.Status = servicio.Status;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = "System"; 
                    
                _dbSet.Update(existing);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Servicio con ID {existing.Id} actualizado correctamente.");

                return existing;
            });
        });
    }
}