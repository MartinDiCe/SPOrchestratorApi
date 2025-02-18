using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Models.Repositories.ContinueWithRepositories
{
    /// <summary>
    /// Implementación reactiva del repositorio para la entidad ServicioContinueWith.
    /// </summary>
    public class ServicioContinueWithRepository : IServicioContinueWithRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService<ServicioContinueWithRepository> _logger;
        private readonly DbSet<ServicioContinueWith> _dbSet;
        private readonly IServiceExecutor _executor;

        public ServicioContinueWithRepository(ApplicationDbContext context,
            ILoggerService<ServicioContinueWithRepository> logger,
            IServiceExecutor executor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _dbSet = _context.Set<ServicioContinueWith>();
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> GetByIdAsync(int id)
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Consultando ServicioContinueWith con ID {id}.");
                    var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
                    if (entity == null)
                        throw new ResourceNotFoundException($"No se encontró ServicioContinueWith con ID {id}.");
                    return entity;
                });
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> CreateAsync(ServicioContinueWith entity)
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo("Creando un nuevo ServicioContinueWith...");
                    _dbSet.Add(entity);
                    await _context.SaveChangesAsync();
                    _logger.LogInfo($"ServicioContinueWith creado con ID {entity.Id}.");
                    return entity;
                });
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> UpdateAsync(ServicioContinueWith entity)
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    if (entity.Id <= 0)
                        throw new ArgumentException("El ID debe ser mayor a 0 para actualizar.", nameof(entity.Id));

                    _logger.LogInfo($"Actualizando ServicioContinueWith con ID {entity.Id}...");
                    var existing = await _dbSet.FirstOrDefaultAsync(x => x.Id == entity.Id && !x.Deleted);
                    if (existing == null)
                        throw new ResourceNotFoundException($"No se encontró ServicioContinueWith con ID {entity.Id}.");
                    
                    existing.ServicioConfiguracionId = entity.ServicioConfiguracionId;
                    existing.ServicioContinuacionId = entity.ServicioContinuacionId;
                    existing.CamposRelacion = entity.CamposRelacion;

                    _dbSet.Update(existing);
                    await _context.SaveChangesAsync();
                    _logger.LogInfo($"ServicioContinueWith con ID {existing.Id} actualizado correctamente.");
                    return existing;
                });
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> SoftDeleteAsync(int id)
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Aplicando soft-delete a ServicioContinueWith con ID {id}.");
                    var entity = await _dbSet.FindAsync(id);
                    if (entity == null)
                        throw new ResourceNotFoundException($"No se encontró ServicioContinueWith con ID {id}.");

                    entity.Deleted = true;
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedBy = "System";
                    _dbSet.Update(entity);
                    await _context.SaveChangesAsync();
                    _logger.LogInfo($"ServicioContinueWith con ID {id} marcado como eliminado.");
                    return entity;
                });
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> RestoreAsync(int id)
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Restaurando ServicioContinueWith con ID {id}.");
                    var entity = await _dbSet.FindAsync(id);
                    if (entity == null)
                        throw new ResourceNotFoundException($"No se encontró ServicioContinueWith con ID {id}.");

                    entity.Deleted = false;
                    entity.DeletedAt = null;
                    entity.DeletedBy = null;
                    _dbSet.Update(entity);
                    await _context.SaveChangesAsync();
                    _logger.LogInfo($"ServicioContinueWith con ID {id} restaurado.");
                    return entity;
                });
            });
        }
        
        /// <inheritdoc />
        public IObservable<IList<ServicioContinueWith>> GetAllAsync()
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo("Consultando todas las entidades de ServicioContinueWith no eliminadas...");
                    var list = await _dbSet.Where(x => !x.Deleted).ToListAsync();
                    _logger.LogInfo($"Se obtuvieron {list.Count} registros.");
                    return list;
                });
            });
        }
        
        /// <inheritdoc />
        public IObservable<IList<ServicioContinueWith>> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId)
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Consultando ServicioContinueWith para ServicioConfiguracionId = {servicioConfiguracionId}.");
                    var list = await _dbSet.Where(x => x.ServicioConfiguracionId == servicioConfiguracionId && !x.Deleted).ToListAsync();
                    _logger.LogInfo($"Se obtuvieron {list.Count} registros.");
                    return list;
                });
            });
        }
    }
}
