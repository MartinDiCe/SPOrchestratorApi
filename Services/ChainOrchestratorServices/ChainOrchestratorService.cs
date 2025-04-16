using System.Reactive.Linq;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.AuditServices;
using SPOrchestratorAPI.Services.ContinueWithServices;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;
using SPOrchestratorAPI.Services.SPOrchestratorServices;

namespace SPOrchestratorAPI.Services.ChainOrchestratorServices
{
    /// <summary>
    /// Servicio que implementa la lógica de encadenamiento de servicios.
    /// Se encarga de invocar el servicio principal, verificar si tiene continuidad y, en caso afirmativo,
    /// transformar el resultado para ejecutar el siguiente servicio. Además, aplica fan‑out de forma transparente
    /// si el resultado es una colección.
    /// </summary>
    public class ChainOrchestratorService(
        ISpOrchestratorService spOrchestratorService,
        IServicioService servicioService,
        IContinuidadHelper continuidadHelper,
        IServicioConfiguracionService configService,
        IServicioContinueWithService continueWithService,
        ILoggerService<ChainOrchestratorService> logger,
        IServiceExecutor executor,
        IAuditoriaService auditoriaService,
        IServiceScopeFactory scopeFactory)
        : IChainOrchestratorService
    {   
        private readonly IContinuidadHelper _continuidadHelper =
            continuidadHelper ?? throw new ArgumentNullException(nameof(continuidadHelper));
        private readonly ISpOrchestratorService _spOrchestratorService =
            spOrchestratorService ?? throw new ArgumentNullException(nameof(spOrchestratorService));

        private readonly IServicioService _servicioService =
            servicioService ?? throw new ArgumentNullException(nameof(servicioService));

        private readonly IServicioConfiguracionService _configService =
            configService ?? throw new ArgumentNullException(nameof(configService));

        private readonly IServicioContinueWithService _continueWithService =
            continueWithService ?? throw new ArgumentNullException(nameof(continueWithService));

        private readonly ILoggerService<ChainOrchestratorService> _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));

        private readonly IServiceExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));

        private readonly IAuditoriaService _auditoriaService =
            auditoriaService ?? throw new ArgumentNullException(nameof(auditoriaService));

        private readonly IServiceScopeFactory _scopeFactory =
            scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

        public IObservable<object> EjecutarConContinuacionAsync(string serviceName,
            IDictionary<string, object>? parameters = null)
        {
            return _executor.ExecuteAsync(() =>
            {
                _logger.LogInfo(
                    $"Ejecutando servicio principal '{serviceName}' con parámetros iniciales: {System.Text.Json.JsonSerializer.Serialize(parameters)}");

                return _spOrchestratorService.EjecutarPorNombreAsync(serviceName, parameters)
                    .SelectMany(result =>
                    {
                        // Aquí usamos el helper para verificar la continuidad
                        return Observable.FromAsync(async () =>
                            {
                                var tieneContinuidad =
                                    await _continuidadHelper.TieneContinuidadConfiguradaAsync(serviceName);
                                return tieneContinuidad;
                            })
                            .SelectMany(tieneContinuidad =>
                            {
                                if (!tieneContinuidad)
                                {
                                    _logger.LogInfo(
                                        $"El servicio '{serviceName}' no tiene continuidad configurada. Se retorna el resultado sin encadenamiento.");
                                    return Observable.Return(result);
                                }

                                if (result is IEnumerable<object> resultList)
                                {
                                    _logger.LogInfo(
                                        $"El servicio '{serviceName}' devolvió una colección con {resultList.Count()} elementos. Aplicando fan‑out.");
                                    return resultList.ToObservable()
                                        .SelectMany(item => ProcessContinuationAsync(serviceName, item));
                                }
                                else
                                {
                                    _logger.LogInfo(
                                        $"El servicio '{serviceName}' devolvió un único resultado. Procesando continuidad.");
                                    return ProcessContinuationAsync(serviceName, result);
                                }
                            });
                    });
            });
        }

        private IObservable<object> ProcessContinuationAsync(string serviceName, object result)
        {
            // Diccionario para almacenar información de auditoría y de la continuidad
            Dictionary<string, object> auditData = new Dictionary<string, object>();

            return _servicioService.GetByNameAsync(serviceName).FirstAsync()
                .SelectMany(servicio =>
                {
                    if (servicio == null)
                    {
                        _logger.LogError($"Servicio '{serviceName}' no encontrado.");
                        return Observable.Throw<object>(
                            new ResourceNotFoundException(
                                $"No se encontró un servicio con el nombre '{serviceName}'."));
                    }

                    auditData["ServicioPadreId"] = servicio.Id;

                    return _configService.GetByServicioIdAsync(servicio.Id).FirstAsync()
                        .SelectMany(configs =>
                        {
                            // Si no existe configuración o no hay mapeo para continuidad, se retorna el resultado sin encadenar.
                            if (configs == null || configs.Count == 0)
                            {
                                _logger.LogInfo(
                                    $"No se encontró configuración para el servicio '{serviceName}', retornando resultado sin continuidad.");
                                return Observable.Return(result);
                            }

                            var config = configs[0];
                            auditData["ConfigPadreId"] = config.Id;

                            // Si la configuración no indica continuidad, se retorna el resultado original.
                            if (!config.ContinuarCon)
                            {
                                _logger.LogInfo(
                                    $"El servicio '{serviceName}' no está configurado para continuar, retornando resultado.");
                                return Observable.Return(result);
                            }

                            // En caso de existir mapeo para continuidad, se procede a encadenar el siguiente servicio.
                            var continuationObservable = _continueWithService
                                .GetByServicioConfiguracionIdAsync(config.Id)
                                .SelectMany(mapeos =>
                                {
                                    if (mapeos == null || mapeos.Count == 0)
                                    {
                                        _logger.LogInfo(
                                            $"No existen mapeos de continuidad para el servicio '{serviceName}', retornando resultado.");
                                        return Observable.Return(result);
                                    }

                                    var mapeo = mapeos[0];
                                    _logger.LogInfo(
                                        $"Aplicando mapeo de continuidad para el servicio '{serviceName}'. Mapping: {mapeo.CamposRelacion}");

                                    auditData["ServicioContinuacionId"] = mapeo.ServicioContinuacionId;
                                    auditData["ConfigContinuacionId"] = mapeo.ServicioContinuacionId;

                                    var parametrosContinuacion =
                                        MappingContinueWithHelper.TransformarResultado(result, mapeo.CamposRelacion,
                                            serviceName);
                                    _logger.LogInfo(
                                        $"Parámetros generados para la continuidad: {System.Text.Json.JsonSerializer.Serialize(parametrosContinuacion)}");

                                    auditData["Mapping"] = mapeo.CamposRelacion;
                                    auditData["ParametrosContinuacion"] = parametrosContinuacion;

                                    return _configService.GetByIdAsync(mapeo.ServicioContinuacionId)
                                        .SelectMany(configContinuacion =>
                                        {
                                            if (configContinuacion == null)
                                            {
                                                _logger.LogError(
                                                    $"Configuración de continuidad no encontrada para ID {mapeo.ServicioContinuacionId}.");
                                                return Observable.Throw<object>(
                                                    new ResourceNotFoundException(
                                                        $"No existe configuración de continuidad con ID {mapeo.ServicioContinuacionId}."));
                                            }

                                            var nextServiceName = configContinuacion.Servicio.Name;

                                            auditData["ServicioContinuacionId"] = configContinuacion.Servicio.Id;
                                            auditData["ConfigContinuacionId"] = configContinuacion.Id;

                                            _logger.LogInfo(
                                                $"Ejecutando servicio de continuidad '{nextServiceName}' con parámetros: {System.Text.Json.JsonSerializer.Serialize(parametrosContinuacion)}");

                                            return EjecutarConContinuacionAsync(nextServiceName,
                                                parametrosContinuacion);
                                        });
                                });

                            return continuationObservable.Catch<object, Exception>(ex =>
                            {
                                _logger.LogError(
                                    $"Error en la continuidad del servicio '{serviceName}' (servicio padre). Se devuelve el resultado original. Error: {ex.Message}");

                                // Registrar auditoría de fallo en la continuidad
                                var auditoriaContinuacion = new ServicioEjecucion
                                {
                                    ServicioId = auditData.ContainsKey("ServicioContinuacionId") &&
                                                 (int)auditData["ServicioContinuacionId"] != 0
                                        ? (int)auditData["ServicioContinuacionId"]
                                        : (int)auditData["ServicioPadreId"],
                                    ServicioConfiguracionId = auditData.ContainsKey("ConfigContinuacionId") &&
                                                              (int)auditData["ConfigContinuacionId"] != 0
                                        ? (int)auditData["ConfigContinuacionId"]
                                        : (int)auditData["ConfigPadreId"],
                                    ServicioDesencadenadorId = (int)auditData["ServicioPadreId"],
                                    FechaEjecucion = DateTime.UtcNow,
                                    Duracion = 0,
                                    Estado = false,
                                    MensajeError =
                                        $"Continuidad fallida en '{serviceName}'. Mapping: {auditData.GetValueOrDefault("Mapping", "N/A")}. " +
                                        $"Parámetros calculados: {System.Text.Json.JsonSerializer.Serialize(auditData.GetValueOrDefault("ParametrosContinuacion", new { }))}. " +
                                        $"Error: {ex.Message}",
                                    Parametros = "",
                                    Resultado = null,
                                    CamposExtra = $"Continuidad desencadenada por '{serviceName}'"
                                };

                                try
                                {
                                    // Se crea un nuevo scope para registrar la auditoría
                                    using (var scope = _scopeFactory.CreateScope())
                                    {
                                        var auditoriaServiceScoped =
                                            scope.ServiceProvider.GetRequiredService<IAuditoriaService>();
                                        auditoriaServiceScoped.RegistrarEjecucionAsync(auditoriaContinuacion).Wait();
                                    }
                                }
                                catch (Exception auditEx)
                                {
                                    _logger.LogError(
                                        $"Error al registrar auditoría de continuidad fallida: {auditEx.Message}");
                                }

                                return Observable.Return(result);
                            });
                        });
                });
        }
    }
}