using System.Reactive.Linq;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.DTOs.ServicioConfiguracionDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Enums;
using SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioServices;

namespace SPOrchestratorAPI.Services.ServicioConfiguracionServices
{
    /// <summary>
    /// Implementación de la lógica de negocio para <see cref="ServicioConfiguracion"/>.
    /// </summary>
    public class ServicioConfiguracionService(
        IServicioConfiguracionRepository repository,
        ILoggerService<ServicioConfiguracionService> logger,
        IServiceExecutor executor,
        IServicioService servicioService)
        : IServicioConfiguracionService
    {
        private readonly IServicioConfiguracionRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILoggerService<ServicioConfiguracionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        private readonly IServicioService _servicioService = servicioService ?? throw new ArgumentNullException(nameof(servicioService));

        /// <inheritdoc />
        public IObservable<ServicioConfiguracion> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor a 0.", nameof(id));
            }

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Buscando configuración con ID {id}...");
                return _repository.GetByIdAsync(id);
            });
        }

        /// <inheritdoc />
        public IObservable<ServicioConfiguracion> CreateAsync(CreateServicioConfiguracionDto dto)
        {
            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo("Creando nueva configuración de servicio...");

                if (dto.ServicioId <= 0)
                {
                    throw new ArgumentException("El ServicioId debe ser mayor a 0.", nameof(dto.ServicioId));
                }

                if (string.IsNullOrWhiteSpace(dto.NombreProcedimiento))
                {
                    throw new ArgumentException("El NombreProcedimiento es obligatorio.",
                        nameof(dto.NombreProcedimiento));
                }

                string? parametrosJson;
                try
                {
                    parametrosJson = ParametrosHelper.ValidarYTransformar(dto.Parametros);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Error en el campo Parametros: {ex.Message}");
                }

                // Validamos que el servicio existe
                return _servicioService.GetByIdAsync(dto.ServicioId)
                    .SelectMany(servicioExistente =>
                    {
                        if (servicioExistente == null)
                        {
                            throw new ArgumentException($"El servicio con ID {dto.ServicioId} no existe.");
                        }

                        return _repository.GetByServicioIdAsync(dto.ServicioId)
                            .SelectMany(existingConfigs =>
                            {
                                if (existingConfigs != null && existingConfigs.Any())
                                {
                                    throw new InvalidOperationException(
                                        $"Ya existe una configuración para el servicio con ID {dto.ServicioId}.");
                                }

                                if (dto.Tipo == TipoConfiguracion.EndPoint)
                                {
                                    try
                                    {
                                        var configValues = EndpointConfigHelper.ParseAndValidateConfig(dto.JsonConfig);
                                    }
                                    catch (FormatException ex)
                                    {
                                        throw new ArgumentException($"Error en el campo JsonConfig: {ex.Message}",
                                            nameof(dto.JsonConfig));
                                    }
                                }

                                var config = new ServicioConfiguracion
                                {
                                    ServicioId = dto.ServicioId,
                                    NombreProcedimiento = dto.NombreProcedimiento,
                                    ConexionBaseDatos = dto.ConexionBaseDatos,
                                    Parametros = parametrosJson,
                                    MaxReintentos = dto.MaxReintentos,
                                    TimeoutSegundos = dto.TimeoutSegundos,
                                    Tipo = dto.Tipo,
                                    Provider = dto.Provider,
                                    EsProgramado = dto.EsProgramado,
                                    ContinuarCon = dto.ContinuarCon,
                                    GuardarRegistros = dto.GuardarRegistros,
                                    JsonConfig = dto.JsonConfig,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = "System",
                                    Servicio = servicioExistente
                                };

                                return _repository
                                    .CreateAsync(config)
                                    .Do(created =>
                                    {
                                        _logger.LogInfo(
                                            $"Configuración {created.Id} creada para el servicio {created.ServicioId}.");
                                    });
                            });
                    });
            });
        }

        /// <inheritdoc />
        public IObservable<ServicioConfiguracion> UpdateAsync(UpdateServicioConfiguracionDto dto)
        {
            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Actualizando configuración con ID {dto.Id}...");

                if (dto.Id <= 0)
                {
                    throw new ArgumentException("El ID debe ser mayor a 0.", nameof(dto.Id));
                }

                if (string.IsNullOrWhiteSpace(dto.NombreProcedimiento))
                {
                    throw new ArgumentException("El NombreProcedimiento es obligatorio.",
                        nameof(dto.NombreProcedimiento));
                }

                if (!Enum.IsDefined(typeof(TipoConfiguracion), dto.Tipo))
                {
                    _logger.LogWarning(
                        "El valor de Tipo recibido no es válido. Se asigna el valor por defecto 'StoredProcedure'.");
                    dto.Tipo = TipoConfiguracion.StoredProcedure;
                }

                string? parametrosJson;
                try
                {
                    parametrosJson = ParametrosHelper.ValidarYTransformar(dto.Parametros);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Error en el campo Parametros: {ex.Message}");
                }

                if (dto.Tipo == TipoConfiguracion.EndPoint)
                {
                    try
                    {
                        var configValues = EndpointConfigHelper.ParseAndValidateConfig(dto.JsonConfig);
                        
                    }
                    catch (FormatException ex)
                    {
                        throw new ArgumentException($"Error en el campo JsonConfig: {ex.Message}",
                            nameof(dto.JsonConfig));
                    }
                }

                var configToUpdate = new ServicioConfiguracion
                {
                    Id = dto.Id,
                    ServicioId = dto.ServicioId,
                    NombreProcedimiento = dto.NombreProcedimiento,
                    ConexionBaseDatos = dto.ConexionBaseDatos,
                    Parametros = parametrosJson,
                    MaxReintentos = dto.MaxReintentos,
                    TimeoutSegundos = dto.TimeoutSegundos,
                    Provider = dto.Provider,
                    EsProgramado = dto.EsProgramado,
                    Tipo = dto.Tipo,
                    ContinuarCon = dto.ContinuarCon,
                    GuardarRegistros = dto.GuardarRegistros,
                    JsonConfig = dto.JsonConfig,
                    Servicio = new Servicio { Id = dto.ServicioId }
                };

                return _repository
                    .UpdateAsync(configToUpdate)
                    .Do(updated =>
                    {
                        _logger.LogInfo(
                            $"Configuración {updated.Id} actualizada correctamente para el servicio {updated.ServicioId}.");
                    });
            });
        }


        /// <inheritdoc />
        public IObservable<ServicioConfiguracion> SoftDeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor a 0 para eliminar lógicamente.", nameof(id));
            }

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Solicitando SoftDelete para la configuración {id}...");
                return _repository
                    .SoftDeleteAsync(id)
                    .Do(deleted =>
                    {
                        _logger.LogInfo($"Configuración {deleted.Id} marcada como eliminada (soft-delete).");
                    });
            });
        }
        
        /// <inheritdoc />
        public IObservable<ServicioConfiguracion> RestoreAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor a 0 para restaurar.", nameof(id));
            }

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Restaurando configuración {id}...");
                return _repository
                    .RestoreAsync(id)
                    .Do(restored =>
                    {
                        _logger.LogInfo($"Configuración {restored.Id} restaurada correctamente (Deleted=false).");
                    });
            });
        }
        
        /// <inheritdoc />
        public IObservable<IList<ServicioConfiguracion>> GetAllAsync()
        {
            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo("Buscando todas las configuraciones no eliminadas...");
                return _repository.GetAllAsync();
            });
        }
        
        /// <inheritdoc />
        public IObservable<IList<ServicioConfiguracion>> GetByServicioIdAsync(int servicioId)
        {
            if (servicioId <= 0)
            {
                throw new ArgumentException("El servicioId debe ser mayor a 0.", nameof(servicioId));
            }

            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo($"Buscando configuraciones para ServicioId={servicioId}...");
                return _repository.GetByServicioIdAsync(servicioId);
            });
        }
    }
}
