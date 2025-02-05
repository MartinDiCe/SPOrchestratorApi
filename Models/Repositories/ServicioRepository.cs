using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.Logging;
using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Exceptions;

namespace SPOrchestratorAPI.Models.Repositories
{
    /// <summary>
    /// Implementación de <see cref="IServicioRepository"/> 
    /// para el acceso a datos de la entidad <see cref="Servicio"/> de manera reactiva.
    /// </summary>
    public class ServicioRepository : IServicioRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService<ServicioRepository> _logger;
        private readonly DbSet<Servicio> _dbSet;
        private readonly IServiceExecutor  _serviceExecutor;

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
            _dbSet = _context.Set<Servicio>();
            _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
        }

        /// <inheritdoc />
        public IObservable<IList<Servicio>> GetActiveServicesAsync()
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
        public IObservable<Servicio> GetByNameAsync(string name)
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
        public IObservable<Servicio> GetByIdAsync(int id)
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
        public IObservable<Servicio> AddAsync(Servicio servicio)
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
    }
}
