using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.DTOs;
using SPOrchestratorAPI.Services.Logging;

namespace SPOrchestratorAPI.Services
{
    /// <summary>
    /// Implementación del servicio para la gestión de <see cref="Servicio"/>.
    /// </summary>
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly ILoggerService<ServicioService> _logger;
        private readonly IServiceExecutor _serviceExecutor;

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

                // 2) Verificamos si ya existe un servicio con el mismo nombre (no eliminado).
                _logger.LogInfo($"Verificando si ya existe un servicio con el nombre: {servicioDto.Name}");

                // Observa que GetByNameAsync lanza ResourceNotFoundException si no existe.
                // Para la lógica de "no existe => se puede crear", capturamos esa excepción
                // y retornamos null, indicando que el nombre está libre.
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
                            // Si la llamada NO lanza ResourceNotFound,
                            // significa que sí encontró un servicio => duplicado
                            _logger.LogWarning($"Ya existe un servicio con el nombre '{servicioDto.Name}'.");
                            throw new InvalidOperationException($"Ya existe un servicio con el nombre '{servicioDto.Name}'.");
                        }

                        // 3) No existe => podemos crear uno nuevo
                        var servicio = new Servicio
                        {
                            Name = servicioDto.Name,
                            Description = servicioDto.Description,
                            Status = servicioDto.Status,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "System" // O tu usuario actual
                        };

                        // 4) Persistimos el nuevo registro en la base de datos
                        return _servicioRepository.AddAsync(servicio);
                    })
                    // 5) Como AddAsync retorna IObservable<Servicio>, tenemos IObservable<IObservable<Servicio>>,
                    //    así que usamos Switch() para "aplanarlo".
                    .Switch()
                    // 6) Finalmente, podemos hacer algo con el servicio creado (logging, etc.)
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
    }
}
