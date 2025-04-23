using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
    /// Se encarga de invocar el servicio principal, verificar si tiene continuidad y,
    /// en caso afirmativo, transformar el resultado para ejecutar el siguiente servicio.
    /// Aplica fan-out de forma transparente si el resultado es una colección.
    /// </summary>
    public class ChainOrchestratorService : IChainOrchestratorService
    {
        private readonly ILoggerService<ChainOrchestratorService> _logger;
        private readonly IServiceExecutor _executor;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Constructor del servicio de orquestación en cadena.
        /// </summary>
        /// <param name="logger">Servicio de logging genérico.</param>
        /// <param name="executor">Encapsula la ejecución reactiva y captura de errores.</param>
        /// <param name="scopeFactory">Factoría para crear scopes de DI aislados.</param>
        public ChainOrchestratorService(
            ILoggerService<ChainOrchestratorService> logger,
            IServiceExecutor executor,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <summary>
        /// Ejecuta un servicio por nombre y, si está configurado para continuar,
        /// encadena la llamada al siguiente servicio según el mapeo definido.
        /// </summary>
        /// <param name="serviceName">Nombre del servicio a ejecutar.</param>
        /// <param name="parameters">Parámetros iniciales (puede ser null).</param>
        /// <returns>Observable con el resultado o resultados encadenados.</returns>
        public IObservable<object> EjecutarConContinuacionAsync(
            string serviceName,
            IDictionary<string, object>? parameters = null)
        {
            return Observable.Using<object, IServiceScope>(
                () => _scopeFactory.CreateScope(),
                scope =>
                {
                    var spService         = scope.ServiceProvider.GetRequiredService<ISpOrchestratorService>();
                    var continuidadHelper = scope.ServiceProvider.GetRequiredService<IContinuidadHelper>();

                    _logger.LogInfo(
                        $"[Chain] Iniciando '{serviceName}' con parámetros: {System.Text.Json.JsonSerializer.Serialize(parameters)}");

                    // Ejecuta el servicio principal dentro de un scope aislado
                    return _executor.ExecuteAsync<object>(() =>
                        spService.EjecutarPorNombreAsync(serviceName, parameters)
                        .SelectMany(result =>
                        {
                            return continuidadHelper
                                .TieneContinuidadConfiguradaAsync(serviceName)
                                .ToObservable()
                                .SelectMany(tieneContinuidad =>
                                {
                                    if (!tieneContinuidad)
                                    {
                                        _logger.LogInfo($"[Chain] '{serviceName}' no continúa. Retornando resultado.");
                                        return Observable.Return(result);
                                    }

                                    if (result is IEnumerable<object> list)
                                    {
                                        _logger.LogInfo($"[Chain] Fan-out: {list.Count()} elementos en '{serviceName}'.");
                                        return list.ToObservable()
                                                   .SelectMany(item =>
                                                       ProcessContinuationAsync(serviceName, item)
                                                   );
                                    }

                                    _logger.LogInfo($"[Chain] Continuidad para único resultado en '{serviceName}'.");
                                    return ProcessContinuationAsync(serviceName, result);
                                });
                        })
                    );
                }
            );
        }

        /// <summary>
        /// Procesa la continuidad de un servicio dado su resultado y ejecuta el siguiente paso.
        /// </summary>
        /// <param name="parentServiceName">Servicio padre desde el que se desencadena.</param>
        /// <param name="result">Resultado del servicio padre.</param>
        /// <returns>Observable con el resultado del servicio de continuación o el original en caso de error.</returns>
        private IObservable<object> ProcessContinuationAsync(string parentServiceName, object result)
        {
            return Observable.Using<object, IServiceScope>(
                () => _scopeFactory.CreateScope(),
                scope =>
                {
                    var servicioService  = scope.ServiceProvider.GetRequiredService<IServicioService>();
                    var configService    = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
                    var continueWithSvc  = scope.ServiceProvider.GetRequiredService<IServicioContinueWithService>();
                    var auditoriaService = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

                    var auditData = new Dictionary<string, object>();

                    return servicioService
                        .GetByNameAsync(parentServiceName)
                        .FirstAsync()
                        .SelectMany(serv =>
                        {
                            if (serv == null)
                                throw new ResourceNotFoundException($"Servicio '{parentServiceName}' no encontrado.");

                            auditData["ServicioPadreId"] = serv.Id;
                            return configService.GetByServicioIdAsync(serv.Id).FirstAsync();
                        })
                        .SelectMany(configs =>
                        {
                            if (configs == null || configs.Count == 0)
                                return Observable.Return(result);

                            var cfg = configs[0];
                            if (!cfg.ContinuarCon)
                                return Observable.Return(result);

                            return continueWithSvc
                                .GetByServicioConfiguracionIdAsync(cfg.Id)
                                .SelectMany(mapList =>
                                {
                                    if (mapList == null || mapList.Count == 0)
                                        return Observable.Return(result);

                                    var map = mapList[0];
                                    var nextParams = MappingContinueWithHelper.TransformarResultado(
                                        result, map.CamposRelacion, parentServiceName);

                                    return configService
                                        .GetByIdAsync(map.ServicioContinuacionId)
                                        .SelectMany(nextCfg =>
                                        {
                                            if (nextCfg == null)
                                                throw new ResourceNotFoundException(
                                                    $"Configuración de continuidad no existe: {map.ServicioContinuacionId}");

                                            return EjecutarConContinuacionAsync(nextCfg.Servicio.Name, nextParams);
                                        });
                                });
                        })
                        .Catch<object, Exception>(ex =>
                        {
                            _logger.LogError($"[Chain] Error en continuidad de '{parentServiceName}': {ex.Message}");
                            var exec = new ServicioEjecucion
                            {
                                ServicioId = (int)auditData.GetValueOrDefault("ServicioContinuacionId", auditData["ServicioPadreId"]),
                                ServicioConfiguracionId = (int)auditData.GetValueOrDefault("ConfigContinuacionId", auditData["ConfigPadreId"]),
                                ServicioDesencadenadorId = (int)auditData["ServicioPadreId"],
                                FechaEjecucion = DateTime.UtcNow,
                                Duracion = 0,
                                Estado = false,
                                MensajeError = ex.Message,
                                Parametros = string.Empty,
                                Resultado = null,
                                CamposExtra = $"Error en continuidad de '{parentServiceName}'"
                            };
                            try
                            {
                                auditoriaService.RegistrarEjecucionAsync(exec).Wait();
                            }
                            catch (Exception logEx)
                            {
                                _logger.LogError($"[Chain] Error registrando auditoría: {logEx.Message}");
                            }
                            return Observable.Return(result);
                        });
                }
            );
        }
    }
}
