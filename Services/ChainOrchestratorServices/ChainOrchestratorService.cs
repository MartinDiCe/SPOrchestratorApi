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
        public ChainOrchestratorService(
            ILoggerService<ChainOrchestratorService> logger,
            IServiceExecutor executor,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <inheritdoc />
        public IObservable<object> EjecutarConContinuacionAsync(
            string serviceName,
            IDictionary<string, object>? parameters = null)
        {
            return Observable.Using<object, IServiceScope>(
                () => _scopeFactory.CreateScope(),
                scope =>
                {
                    var spService          = scope.ServiceProvider.GetRequiredService<ISpOrchestratorService>();
                    var continuidadHelper  = scope.ServiceProvider.GetRequiredService<IContinuidadHelper>();
                    var servicioService    = scope.ServiceProvider.GetRequiredService<IServicioService>();
                    var configService      = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
                    var continueWithSvc    = scope.ServiceProvider.GetRequiredService<IServicioContinueWithService>();
                    var auditoriaService   = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

                    _logger.LogInfo($"[Chain] Iniciando '{serviceName}' con parámetros: {System.Text.Json.JsonSerializer.Serialize(parameters)}");

                    return _executor.ExecuteAsync<object>(() =>
                        spService.EjecutarPorNombreAsync(serviceName, parameters, skipAudit: true)
                        .SelectMany(async result =>
                        {
                            // Auditoría del servicio padre
                            var serv = await servicioService.GetByNameAsync(serviceName).FirstAsync();
                            var cfgs = await configService.GetByServicioIdAsync(serv.Id).FirstAsync();
                            var cfg  = cfgs[0];

                            int parentExecId = 0;
                            if (cfg.GuardarRegistros)
                            {
                                var padreEjec = new ServicioEjecucion
                                {
                                    ServicioId                   = serv.Id,
                                    ServicioConfiguracionId      = cfg.Id,
                                    ServicioDesencadenadorId     = null,
                                    FechaEjecucion               = DateTime.UtcNow,
                                    Duracion                     = 0,
                                    Estado                       = true,
                                    MensajeError                 = null,
                                    Parametros                   = System.Text.Json.JsonSerializer.Serialize(parameters),
                                    Resultado                    = System.Text.Json.JsonSerializer.Serialize(result),
                                    CamposExtra                  = null
                                };
                                padreEjec = await auditoriaService.RegistrarEjecucionAsync(padreEjec);
                                parentExecId = padreEjec.Id;
                            }
                            return (result, parentExecId);
                        })
                        .SelectMany(tuple =>
                        {
                            var (result, parentExecId) = tuple;
                            return continuidadHelper.TieneContinuidadConfiguradaAsync(serviceName)
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
                                            .SelectMany(item => ProcessContinuationAsync(serviceName, item, parentExecId));
                                    }
                                    _logger.LogInfo($"[Chain] Continuidad para único resultado en '{serviceName}'.");
                                    return ProcessContinuationAsync(serviceName, result, parentExecId);
                                });
                        })
                    );
                }
            );
        }

        /// <summary>
        /// Procesa la continuidad de un servicio dado su resultado y ejecuta el siguiente paso.
        /// </summary>
        private IObservable<object> ProcessContinuationAsync(
            string parentServiceName,
            object result,
            int parentExecId)
        {
            return Observable.Using<object, IServiceScope>(
                () => _scopeFactory.CreateScope(),
                scope =>
                {
                    var spService        = scope.ServiceProvider.GetRequiredService<ISpOrchestratorService>();
                    var servicioService  = scope.ServiceProvider.GetRequiredService<IServicioService>();
                    var configService    = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
                    var continueWithSvc  = scope.ServiceProvider.GetRequiredService<IServicioContinueWithService>();
                    var auditoriaService = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

                    var auditData = new Dictionary<string, object> { ["ServicioDesencadenadorExecId"] = parentExecId };

                    return servicioService.GetByNameAsync(parentServiceName)
                        .FirstAsync()
                        .SelectMany(serv =>
                        {
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

                            return continueWithSvc.GetByServicioConfiguracionIdAsync(cfg.Id)
                                .SelectMany(mapList =>
                                {
                                    if (mapList == null || mapList.Count == 0)
                                        return Observable.Return(result);

                                    var map        = mapList[0];
                                    var nextParams = MappingContinueWithHelper.TransformarResultado(result, map.CamposRelacion, parentServiceName);

                                    return configService.GetByIdAsync(map.ServicioContinuacionId)
                                        .SelectMany(nextCfg =>
                                        {
                                            var childExec = new ServicioEjecucion
                                            {
                                                ServicioId                        = nextCfg.Servicio.Id,
                                                ServicioConfiguracionId           = nextCfg.Id,
                                                ServicioDesencadenadorId          = (int)auditData["ServicioPadreId"],
                                                ServicioEjecucionDesencadenadorId = parentExecId,
                                                FechaEjecucion                    = DateTime.UtcNow,
                                                Duracion                          = 0,
                                                Estado                            = true,
                                                MensajeError                      = null,
                                                Parametros                        = System.Text.Json.JsonSerializer.Serialize(nextParams),
                                                Resultado                         = null,
                                                CamposExtra                       = $"Encadenado desde '{parentServiceName}'"
                                            };
                                            return Observable.FromAsync(async () =>
                                            {
                                                childExec = await auditoriaService.RegistrarEjecucionAsync(childExec);
                                                return await spService.EjecutarPorNombreAsync(nextCfg.Servicio.Name, nextParams, skipAudit: true).FirstAsync();
                                            });
                                        });
                                });
                        })
                        .Catch<object, Exception>(ex =>
                        {
                            _logger.LogError($"[Chain] Error en continuidad de '{parentServiceName}': {ex.Message}");
                            return Observable.Return(result);
                        });
                }
            );
        }
    }
}
