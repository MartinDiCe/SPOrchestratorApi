using System.Reactive.Linq;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.DTOs.ParameterDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Services.ParameterServices
{
/// <summary>
    /// Implementación del servicio para la gestión de parámetros globales.
    /// Utiliza un <see cref="IServiceExecutor"/> para encapsular la lógica de ejecución reactiva
    /// y un servicio de logging para registrar la actividad.
    /// </summary>
    public class ParameterService : IParameterService
    {
        private readonly IParameterRepository _parameterRepository;
        private readonly ILoggerService<ParameterService> _logger;
        private readonly IServiceExecutor _serviceExecutor;

        /// <summary>
        /// Crea una nueva instancia de <see cref="ParameterService"/>.
        /// </summary>
        /// <param name="parameterRepository">Repositorio para acceder a los datos de la entidad <see cref="Parameter"/>.</param>
        /// <param name="logger">Servicio de logging para registrar actividad y errores.</param>
        /// <param name="serviceExecutor">Ejecutor reactivo para manejar errores y suscripciones.</param>
        /// <exception cref="ArgumentNullException">Si algún parámetro es nulo.</exception>
        public ParameterService(
            IParameterRepository parameterRepository,
            ILoggerService<ParameterService> logger,
            IServiceExecutor serviceExecutor)
        {
            _parameterRepository = parameterRepository ?? throw new ArgumentNullException(nameof(parameterRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
        }

        /// <inheritdoc />
        public IObservable<Parameter> CreateAsync(CreateParameterDto dto)
        {
            return _serviceExecutor.ExecuteAsync<Parameter>(() =>
            {
                _logger.LogInfo($"Iniciando la creación del parámetro global: {dto.ParameterName}");

                // Validaciones de entrada.
                if (string.IsNullOrWhiteSpace(dto.ParameterName))
                {
                    _logger.LogWarning("El nombre del parámetro es obligatorio.");
                    throw new ArgumentException("El nombre del parámetro es obligatorio.", nameof(dto.ParameterName));
                }
                if (string.IsNullOrWhiteSpace(dto.ParameterValue))
                {
                    _logger.LogWarning("El valor del parámetro es obligatorio.");
                    throw new ArgumentException("El valor del parámetro es obligatorio.", nameof(dto.ParameterValue));
                }
                
                if (string.IsNullOrWhiteSpace(dto.ParameterCategory))
                {
                    _logger.LogWarning("El valor del parámetro es obligatorio.");
                    throw new ArgumentException("La categoría del parámetro es obligatorio.", nameof(dto.ParameterValue));
                }

                var parameter = new Parameter
                {
                    ParameterName = dto.ParameterName,
                    ParameterValue = dto.ParameterValue,
                    ParameterDescription = dto.ParameterDescription,
                    ParameterCategory = dto.ParameterCategory,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _logger.LogInfo($"Creando el parámetro: {parameter.ParameterName}");

                // Convertimos el Task en IObservable usando Observable.FromAsync.
                return Observable.FromAsync(() => _parameterRepository.CreateAsync(parameter));
            });
        }

        /// <inheritdoc />
        public IObservable<Parameter?> GetByIdAsync(int parameterId)
        {
            if (parameterId <= 0)
            {
                throw new ArgumentException("El ID del parámetro debe ser mayor que 0.", nameof(parameterId));
            }

            return _serviceExecutor.ExecuteAsync<Parameter?>(() =>
            {
                _logger.LogInfo($"Buscando parámetro con ID {parameterId}.");
                return Observable.FromAsync(() => _parameterRepository.GetByIdAsync(parameterId));
            });
        }

        /// <inheritdoc />
        public IObservable<Parameter?> GetByNameAsync(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("El nombre del parámetro es obligatorio.", nameof(parameterName));
            }

            return _serviceExecutor.ExecuteAsync<Parameter?>(() =>
            {
                _logger.LogInfo($"Buscando parámetro con el nombre '{parameterName}'.");
                return Observable.FromAsync(() => _parameterRepository.GetByNameAsync(parameterName));
            });
        }
    }
}