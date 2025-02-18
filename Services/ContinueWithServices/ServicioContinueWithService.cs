using System.Reactive.Linq;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.DTOs.ContinueWithDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories.ContinueWithRepositories;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;

namespace SPOrchestratorAPI.Services.ContinueWithServices
{
    public class ServicioContinueWithService(
        IServicioContinueWithRepository repository,
        ILoggerService<ServicioContinueWithService> logger,
        IServiceExecutor executor,
        IServicioConfiguracionService configService)
        : IServicioContinueWithService
    {
        private readonly IServicioContinueWithRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILoggerService<ServicioContinueWithService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        private readonly IServicioConfiguracionService _configService = configService ?? throw new ArgumentNullException(nameof(configService)); // Para obtener la configuración del servicio de continuación

        /// <inheritdoc />
        public IObservable<ServicioContinueWith> CreateAsync(CreateServicioContinueWithDto dto)
        {
            return _executor.ExecuteAsync(() =>
            {
                // Validar el ID del servicio de continuación.
                if (dto.ServicioContinuacionId <= 0)
                    throw new ArgumentException("El ID del servicio de continuación debe ser mayor a 0.", nameof(dto.ServicioContinuacionId));

                return _configService.GetByServicioIdAsync(dto.ServicioContinuacionId)
                    .SelectMany(configList =>
                    {
                        if (configList == null || configList.Count == 0)
                            throw new ResourceNotFoundException($"No se encontró configuración para el servicio de continuación con ID {dto.ServicioContinuacionId}.");

                        var configContinuacion = configList[0];
                        var parametrosEsperados = ConfiguracionHelper.ObtenerParametrosEsperados(configContinuacion.Parametros);

                        if (string.IsNullOrWhiteSpace(dto.CamposRelacion))
                            throw new ArgumentException("La cadena de mapeo (CamposRelacion) es obligatoria.", nameof(dto.CamposRelacion));

                        try
                        {
                            MappingContinueWithHelper.ValidateAndParseMapping(dto.CamposRelacion, parametrosEsperados);
                        }
                        catch (FormatException ex)
                        {
                            throw new ArgumentException($"Error en la cadena de mapeo: {ex.Message}", nameof(dto.CamposRelacion));
                        }
                        
                        var entity = new ServicioContinueWith
                        {
                            ServicioConfiguracionId = dto.ServicioConfiguracionId,
                            ServicioContinuacionId = dto.ServicioContinuacionId,
                            CamposRelacion = dto.CamposRelacion,
                            // Los campos de auditoría se establecerán automáticamente (por ejemplo, en el contexto o en el servicio de auditoría)
                        };

                        _logger.LogInfo("Creando un nuevo mapeo de continuación...");
                        return _repository.CreateAsync(entity)
                            .Do(created => _logger.LogInfo($"Mapeo de continuación creado con ID {created.Id}."));
                    });
            });
        }

        /// <inheritdoc />
        public IObservable<ServicioContinueWith> UpdateAsync(UpdateServicioContinueWithDto dto)
        {
            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Actualizando mapeo de continuación con ID {dto.Id}...");

                if (dto.Id <= 0)
                    throw new ArgumentException("El ID debe ser mayor a 0.", nameof(dto.Id));
                if (dto.ServicioContinuacionId <= 0)
                    throw new ArgumentException("El ID del servicio de continuación debe ser mayor a 0.", nameof(dto.ServicioContinuacionId));

                return _configService.GetByServicioIdAsync(dto.ServicioContinuacionId)
                    .SelectMany(configList =>
                    {
                        if (configList == null || configList.Count == 0)
                            throw new ResourceNotFoundException($"No se encontró configuración para el servicio de continuación con ID {dto.ServicioContinuacionId}.");

                        var configContinuacion = configList[0];
                        var parametrosEsperados = ConfiguracionHelper.ObtenerParametrosEsperados(configContinuacion.Parametros);

                        if (string.IsNullOrWhiteSpace(dto.CamposRelacion))
                            throw new ArgumentException("La cadena de mapeo (CamposRelacion) es obligatoria.", nameof(dto.CamposRelacion));

                        try
                        {
                            MappingContinueWithHelper.ValidateAndParseMapping(dto.CamposRelacion, parametrosEsperados);
                        }
                        catch (FormatException ex)
                        {
                            throw new ArgumentException($"Error en la cadena de mapeo: {ex.Message}", nameof(dto.CamposRelacion));
                        }

                        // Mapear el DTO a la entidad.
                        var entity = new ServicioContinueWith
                        {
                            Id = dto.Id,
                            ServicioConfiguracionId = dto.ServicioConfiguracionId,
                            ServicioContinuacionId = dto.ServicioContinuacionId,
                            CamposRelacion = dto.CamposRelacion
                        };

                        return _repository.UpdateAsync(entity)
                            .Do(updated => _logger.LogInfo($"Mapeo de continuación actualizado con ID {updated.Id}."));
                    });
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0.", nameof(id));

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Consultando mapeo de continuación con ID {id}...");
                return _repository.GetByIdAsync(id);
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> SoftDeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0 para eliminar.", nameof(id));

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Aplicando soft-delete al mapeo de continuación con ID {id}...");
                return _repository.SoftDeleteAsync(id)
                    .Do(deleted => _logger.LogInfo($"Mapeo de continuación con ID {deleted.Id} marcado como eliminado."));
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioContinueWith> RestoreAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0 para restaurar.", nameof(id));

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Restaurando mapeo de continuación con ID {id}...");
                return _repository.RestoreAsync(id)
                    .Do(restored => _logger.LogInfo($"Mapeo de continuación con ID {restored.Id} restaurado."));
            });
        }
        
        /// <inheritdoc />
        public IObservable<IList<ServicioContinueWith>> GetAllAsync()
        {
            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo("Consultando todos los mapeos de continuación no eliminados...");
                return _repository.GetAllAsync()
                    .Do(list => _logger.LogInfo($"Se obtuvieron {list.Count} registros de mapeos de continuación."));
            });
        }
        
        /// <inheritdoc />
        public IObservable<IList<ServicioContinueWith>> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId)
        {
            if (servicioConfiguracionId <= 0)
                throw new ArgumentException("El ID de configuración debe ser mayor a 0.", nameof(servicioConfiguracionId));

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Consultando mapeos de continuación para ServicioConfiguracionId {servicioConfiguracionId}...");
                return _repository.GetByServicioConfiguracionIdAsync(servicioConfiguracionId)
                    .Do(list => _logger.LogInfo($"Se obtuvieron {list.Count} mapeos para ServicioConfiguracionId {servicioConfiguracionId}."));
            });
        }
    }
}