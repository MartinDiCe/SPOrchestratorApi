using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Models.Repositories.ServicioProgramacionRepositories
{
    /// <summary>
    /// Implementación de <see cref="IServicioProgramacionRepository"/>
    /// para la entidad <see cref="ServicioProgramacion"/> de manera reactiva.
    /// </summary>
    public class ServicioProgramacionRepository : IServicioProgramacionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService<ServicioProgramacionRepository> _logger;
        private readonly DbSet<ServicioProgramacion> _dbSet;
        private readonly IServiceExecutor _serviceExecutor;

        public ServicioProgramacionRepository(
            ApplicationDbContext context,
            ILoggerService<ServicioProgramacionRepository> logger,
            IServiceExecutor serviceExecutor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<ServicioProgramacion>();
            _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> GetByIdAsync(int id)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Consultando programación con ID {id}...");
                    var programacion = await _dbSet
                        .Where(p => p.Id == id && !p.Deleted)
                        .Include(p => p.ServicioConfiguracion) // Incluye la configuración asociada si es necesario
                        .FirstOrDefaultAsync();

                    if (programacion == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró la programación con ID {id}.");
                    }
                    return programacion;
                });
            });
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> CreateAsync(ServicioProgramacion programacion)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo("Creando una nueva programación de servicio...");
                    
                    _dbSet.Add(programacion);
                    await _context.SaveChangesAsync();

                    _logger.LogInfo($"Programación creada con ID {programacion.Id} para la configuración con ID {programacion.ServicioConfiguracionId}.");
                    return programacion;
                });
            });
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> UpdateAsync(ServicioProgramacion programacion)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    if (programacion.Id <= 0)
                    {
                        throw new ArgumentException("El ID debe ser mayor que 0 para actualizar.", nameof(programacion.Id));
                    }

                    _logger.LogInfo($"Actualizando programación con ID {programacion.Id}...");
                    var existing = await _dbSet
                        .Where(p => p.Id == programacion.Id && !p.Deleted)
                        .FirstOrDefaultAsync();

                    if (existing == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró la programación con ID {programacion.Id}.");
                    }

                    existing.ServicioConfiguracionId = programacion.ServicioConfiguracionId;
                    existing.CronExpression = programacion.CronExpression;
                    existing.StartDate = programacion.StartDate;
                    existing.EndDate = programacion.EndDate;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = "System";

                    _dbSet.Update(existing);
                    await _context.SaveChangesAsync();
                    _logger.LogInfo($"Programación con ID {existing.Id} actualizada correctamente.");

                    return existing;
                });
            });
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> SoftDeleteAsync(int id)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Aplicando soft-delete a la programación con ID {id}...");
                    var programacion = await _dbSet.FindAsync(id);

                    if (programacion == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró la programación con ID {id}.");
                    }

                    programacion.Deleted = true;
                    programacion.DeletedAt = DateTime.UtcNow;
                    programacion.DeletedBy = "System";
                    _dbSet.Update(programacion);
                    await _context.SaveChangesAsync();

                    _logger.LogInfo($"Programación con ID {id} marcada como eliminada.");
                    return programacion;
                });
            });
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> RestoreAsync(int id)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Restaurando la programación con ID {id} (soft-delete)...");
                    var programacion = await _dbSet.FindAsync(id);

                    if (programacion == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró la programación con ID {id}.");
                    }

                    programacion.Deleted = false;
                    programacion.DeletedAt = null;
                    programacion.DeletedBy = null;
                    _dbSet.Update(programacion);
                    await _context.SaveChangesAsync();

                    _logger.LogInfo($"Programación con ID {id} restaurada.");
                    return programacion;
                });
            });
        }

        /// <inheritdoc />
        public IObservable<IList<ServicioProgramacion>> GetAllAsync()
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo("Consultando todas las programaciones no eliminadas...");
                    var programaciones = await _dbSet
                        .Where(p => !p.Deleted)
                        .Include(p => p.ServicioConfiguracion)
                        .ToListAsync();

                    _logger.LogInfo($"Se obtuvieron {programaciones.Count} programaciones.");
                    return programaciones;
                });
            });
        }

        /// <inheritdoc />
        public IObservable<IList<ServicioProgramacion>> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Consultando programaciones para la configuración con ID {servicioConfiguracionId}...");
                    var programaciones = await _dbSet
                        .Where(p => p.ServicioConfiguracionId == servicioConfiguracionId && !p.Deleted)
                        .Include(p => p.ServicioConfiguracion)
                        .ToListAsync();

                    _logger.LogInfo($"Se obtuvieron {programaciones.Count} programaciones para la configuración con ID {servicioConfiguracionId}.");
                    return programaciones;
                });
            });
        }
    }
}
