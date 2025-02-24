using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;

/// <summary>
/// Implementación de <see cref="IServicioConfiguracionRepository"/> 
/// para la entidad <see cref="ServicioConfiguracion"/> de manera reactiva.
/// </summary>
public class ServicioConfiguracionRepository : IServicioConfiguracionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILoggerService<ServicioConfiguracionRepository> _logger;
    private readonly DbSet<ServicioConfiguracion> _dbSet;
    private readonly IServiceExecutor _serviceExecutor;

    public ServicioConfiguracionRepository(
        ApplicationDbContext context,
        ILoggerService<ServicioConfiguracionRepository> logger,
        IServiceExecutor serviceExecutor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = _context.Set<ServicioConfiguracion>();
        _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
    }
    
    /// <inheritdoc />
    public IObservable<ServicioConfiguracion> GetByIdAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Consultando configuración con ID {id}...");
                var config = await _dbSet
                    .Where(c => c.Id == id && !c.Deleted)
                    .Include(c => c.Servicio) // si deseas incluir la entidad Servicio
                    .FirstOrDefaultAsync();

                if (config == null)
                {
                    throw new ResourceNotFoundException(
                        $"No se encontró la configuración con ID {id}.");
                }
                return config;
            });
        });
    }
    
    /// <inheritdoc />
    public IObservable<ServicioConfiguracion> CreateAsync(ServicioConfiguracion config)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo("Creando una nueva configuración de servicio...");
                _dbSet.Add(config);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Configuración creada con ID {config.Id} para el servicio {config.ServicioId}.");
                return config;
            });
        });
    }
    
    /// <inheritdoc />
    public IObservable<ServicioConfiguracion> UpdateAsync(ServicioConfiguracion config)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                if (config.Id <= 0)
                {
                    throw new ArgumentException("El ID debe ser mayor que 0 para actualizar.", nameof(config.Id));
                }

                _logger.LogInfo($"Actualizando configuración con ID {config.Id}...");
                var existing = await _dbSet
                    .Where(c => c.Id == config.Id && !c.Deleted)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    throw new ResourceNotFoundException($"No se encontró configuración con ID {config.Id}.");
                }
                
                existing.ServicioId = config.ServicioId; 
                existing.NombreProcedimiento = config.NombreProcedimiento;
                existing.ConexionBaseDatos = config.ConexionBaseDatos;
                existing.Parametros = config.Parametros;
                existing.MaxReintentos = config.MaxReintentos;
                existing.TimeoutSegundos = config.TimeoutSegundos;
                existing.Provider = config.Provider;
                existing.Tipo = config.Tipo;
                existing.EsProgramado = config.EsProgramado;
                existing.ContinuarCon = config.ContinuarCon;
                existing.JsonConfig = config.JsonConfig;
                existing.GuardarRegistros = config.GuardarRegistros;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = "System";

                _dbSet.Update(existing);
                await _context.SaveChangesAsync();
                _logger.LogInfo($"Configuración con ID {existing.Id} actualizada correctamente.");

                return existing;
            });
        });
    }
    
    /// <inheritdoc />
    public IObservable<ServicioConfiguracion> SoftDeleteAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Aplicando soft-delete a la configuración con ID {id}...");
                var config = await _dbSet.FindAsync(id);

                if (config == null)
                {
                    throw new ResourceNotFoundException($"No se encontró configuración con ID {id}.");
                }

                config.Deleted = true;
                config.DeletedAt = DateTime.UtcNow;
                config.DeletedBy = "System";
                _dbSet.Update(config);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Configuración con ID {id} marcada como eliminada.");
                return config;
            });
        });
    }
    
    /// <inheritdoc />
    public IObservable<ServicioConfiguracion> RestoreAsync(int id)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Restaurando configuración con ID {id} (soft-delete)...");
                var config = await _dbSet.FindAsync(id);

                if (config == null)
                {
                    throw new ResourceNotFoundException($"No se encontró configuración con ID {id}.");
                }

                config.Deleted = false;
                config.DeletedAt = null;
                config.DeletedBy = null;
                _dbSet.Update(config);
                await _context.SaveChangesAsync();

                _logger.LogInfo($"Configuración con ID {id} restaurada (Deleted=false).");
                return config;
            });
        });
    }
    
    /// <inheritdoc />
    public IObservable<IList<ServicioConfiguracion>> GetAllAsync()
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo("Consultando todas las configuraciones no eliminadas...");
                var configs = await _dbSet
                    .Where(c => !c.Deleted)
                    .Include(c => c.Servicio) // Si deseas traer datos de Servicio
                    .ToListAsync();

                _logger.LogInfo($"Se obtuvieron {configs.Count} configuraciones.");
                return configs;
            });
        });
    }
    
    /// <inheritdoc />
    public IObservable<IList<ServicioConfiguracion>> GetByServicioIdAsync(int servicioId)
    {
        return _serviceExecutor.ExecuteAsync(() =>
        {
            return Observable.FromAsync(async () =>
            {
                _logger.LogInfo($"Consultando configuraciones para el Servicio con ID {servicioId}, no eliminadas...");
                var configs = await _dbSet
                    .Where(c => c.ServicioId == servicioId && !c.Deleted)
                    .Include(c => c.Servicio)
                    .ToListAsync();

                return configs;
            });
        });
    }
}