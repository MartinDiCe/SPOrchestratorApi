﻿using System.Reactive.Linq;
using System.Text.Json;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;

namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    /// <summary>
    /// Servicio para la ejecución de procedimientos almacenados utilizando la configuración 
    /// definida en <see cref="ServicioConfiguracion"/>. Aplica un enfoque reactivo y principios SOLID,
    /// delegando la ejecución según el proveedor a través de una factoría de executors.
    /// </summary>
    public class StoredProcedureService(
        IServicioConfiguracionService configService,
        ILoggerService<StoredProcedureService> logger,
        IServiceExecutor serviceExecutor,
        IServicioService servicioService,
        IStoredProcedureExecutorFactory executorFactory)
        : IStoredProcedureService
    {
        private readonly ILoggerService<StoredProcedureService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceExecutor _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
        private readonly IServicioConfiguracionService _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        private readonly IServicioService _servicioService = servicioService ?? throw new ArgumentNullException(nameof(servicioService));
        private readonly IStoredProcedureExecutorFactory _executorFactory = executorFactory ?? throw new ArgumentNullException(nameof(executorFactory));
        
        /// <inheritdoc />
        public IObservable<object> EjecutarSpConRespuestaPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Iniciando ejecución del stored procedure para el servicio '{serviceName}'.");

                    // Obtener el servicio por su nombre
                    var servicio = await _servicioService.GetByNameAsync(serviceName).FirstAsync();
                    if (servicio == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");
                    }
                    var configs = await _configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                    if (configs == null || configs.Count == 0)
                    {
                        throw new ResourceNotFoundException($"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");
                    }
                    var config = configs[0];
                    if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                    {
                        throw new InvalidOperationException("El nombre del stored procedure no está definido en la configuración.");
                    }
                    _logger.LogInfo($"Configuración obtenida para el servicio '{serviceName}'. Proveedor: {config.Provider}, SP: {config.NombreProcedimiento}");
                    
                    if (!string.IsNullOrWhiteSpace(config.Parametros))
                    {
                        Dictionary<string, string> expectedParams;
                        try
                        {
                            expectedParams = JsonSerializer.Deserialize<Dictionary<string, string>>(config.Parametros)
                                             ?? new Dictionary<string, string>();
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("No se pudo deserializar la configuración de parámetros esperados.", ex);
                        }
                        var expectedValues = new HashSet<string>(expectedParams.Values, StringComparer.OrdinalIgnoreCase);
                        var missingParams = new List<string>();
                        var extraParams = new List<string>();

                        var parametersCI = parameters != null
                            ? new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase)
                            : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                        foreach (var expected in expectedValues)
                        {
                            if (!parametersCI.ContainsKey(expected))
                            {
                                missingParams.Add(expected);
                            }
                        }
                        foreach (var key in parametersCI.Keys)
                        {
                            if (!expectedValues.Contains(key))
                            {
                                extraParams.Add(key);
                            }
                        }
                        if (missingParams.Count > 0)
                        {
                            throw new ArgumentException($"Faltan los siguientes parámetros requeridos: {string.Join(", ", missingParams)}.");
                        }
                        if (extraParams.Count > 0)
                        {
                            throw new ArgumentException($"Se enviaron parámetros no esperados: {string.Join(", ", extraParams)}.");
                        }
                    }

                    var executor = _executorFactory.GetExecutor(config.Provider);
                    object resultData = await executor.ExecuteReaderAsync(config, parameters);

                    _logger.LogInfo("El stored procedure se ejecutó correctamente y se obtuvo la respuesta.");
                    return resultData;
                });
            });
        }
    }
}
