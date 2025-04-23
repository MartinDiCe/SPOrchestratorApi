using System.Reactive.Linq;
using System.Text.Json;
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
    /// <inheritdoc />
    public class ChainOrchestratorService : IChainOrchestratorService
    {
        private readonly ILoggerService<ChainOrchestratorService> _logger;
        private readonly IServiceExecutor _executor;
        private readonly IServiceScopeFactory _scopeFactory;

        public ChainOrchestratorService(
            ILoggerService<ChainOrchestratorService> logger,
            IServiceExecutor executor,
            IServiceScopeFactory scopeFactory)
        {
            _logger       = logger      ?? throw new ArgumentNullException(nameof(logger));
            _executor     = executor    ?? throw new ArgumentNullException(nameof(executor));
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
                    var spService        = scope.ServiceProvider.GetRequiredService<ISpOrchestratorService>();
                    var servicioService  = scope.ServiceProvider.GetRequiredService<IServicioService>();
                    var configService    = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
                    var auditoriaService = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

                    _logger.LogInfo($"[Chain] Iniciando '{serviceName}'");

                    return _executor.ExecuteAsync<object>(() =>
                        spService.EjecutarPorNombreAsync(serviceName, parameters, skipAudit: true)
                        .SelectMany(async result =>
                        {
                            var serv = await servicioService.GetByNameAsync(serviceName).FirstAsync();
                            var cfg  = (await configService.GetByServicioIdAsync(serv.Id).FirstAsync())
                                       .FirstOrDefault();

                            var execId = 0;
                            if (cfg is { GuardarRegistros: true })
                            {
                                var ejec = new ServicioEjecucion
                                {
                                    ServicioId              = serv.Id,
                                    ServicioConfiguracionId = cfg.Id,
                                    FechaEjecucion          = DateTime.UtcNow,
                                    Estado                  = true,
                                    Parametros              = System.Text.Json.JsonSerializer.Serialize(parameters),
                                    Resultado               = System.Text.Json.JsonSerializer.Serialize(result)
                                };
                                execId = (await auditoriaService.RegistrarEjecucionAsync(ejec)).Id;
                            }
                            return (result, execId);
                        })
                        .SelectMany(t =>
                        {
                            var (result, execId) = t;

                            return result is IEnumerable<object> list
                                ? list.ToObservable()
                                      .SelectMany(item =>
                                          ContinueRecursively(
                                              serviceName,
                                              item,
                                              execId,
                                              new HashSet<string> { serviceName }))  // ← copia
                                : ContinueRecursively(
                                      serviceName,
                                      result,
                                      execId,
                                      new HashSet<string> { serviceName });
                        }));
                });
        }

        private IObservable<object> ContinueRecursively(
            string currentService,
            object result,
            int parentExecId,
            HashSet<string> visited)
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

                    return servicioService.GetByNameAsync(currentService)
                        .FirstAsync()
                        .SelectMany(serv => configService.GetByServicioIdAsync(serv.Id).FirstAsync())
                        .Select(cfgs => cfgs.FirstOrDefault())
                        .SelectMany(cfg =>
                        {
                            if (cfg is null || !cfg.ContinuarCon)
                                return Observable.Return<object>(result);

                            return continueWithSvc.GetByServicioConfiguracionIdAsync(cfg.Id)
                                .FirstAsync()
                                .SelectMany(maps => maps.ToObservable())
                                .SelectMany(map =>
                                    HandleMap(map, result, currentService, parentExecId,
                                              visited, spService, configService, auditoriaService));
                        })
                        .Catch<object, ResourceNotFoundException>(ex =>
                        {
                            _logger.LogWarning($"[Chain] {ex.Message}");
                            return Observable.Return<object>(result);
                        })
                        .Catch<object, Exception>(ex =>
                        {
                            _logger.LogError($"[Chain] Error en '{currentService}': {ex.Message}");
                            return Observable.Return<object>(result);
                        });
                });
        }

        private IObservable<object> HandleMap(
            ServicioContinueWith map,
            object parentResult,
            string parentService,
            int parentExecId,
            HashSet<string> visited,
            ISpOrchestratorService spService,
            IServicioConfiguracionService configService,
            IAuditoriaService auditoriaService)
        {
            return configService.GetByIdAsync(map.ServicioContinuacionId)
                .Catch<ServicioConfiguracion, ResourceNotFoundException>(ex =>
                {
                    _logger.LogWarning($"[Chain] Configuración {map.ServicioContinuacionId} no encontrada; se omite la continuación.");
                    return Observable.Empty<ServicioConfiguracion>();
                })
                .SelectMany(nextCfg =>
                {
                    var nextService = nextCfg.Servicio.Name;

                    if (!visited.Add(nextService))
                    {
                        _logger.LogWarning($"[Chain] Ciclo detectado entre '{parentService}' y '{nextService}'.");
                        return Observable.Return<object>(parentResult);
                    }

                    var nextParams = MappingContinueWithHelper.TransformarResultado(
                        parentResult, map.CamposRelacion, parentService);

                    return Observable.FromAsync(async () =>
                        {
                            try
                            {
                                var res = await spService
                                    .EjecutarPorNombreAsync(nextService, nextParams, skipAudit: true)
                                    .FirstAsync();

                                var execOk = new ServicioEjecucion
                                {
                                    ServicioId                        = nextCfg.Servicio.Id,
                                    ServicioConfiguracionId           = nextCfg.Id,
                                    ServicioEjecucionDesencadenadorId = parentExecId,
                                    FechaEjecucion                    = DateTime.UtcNow,
                                    Estado                            = true,
                                    Parametros = System.Text.Json.JsonSerializer.Serialize(nextParams),
                                    Resultado  = System.Text.Json.JsonSerializer.Serialize(res)
                                };
                                execOk = await auditoriaService.RegistrarEjecucionAsync(execOk);

                                return (nextService, (object)res, execOk.Id);
                            }
                            catch (Exception ex)
                            {
                                var execErr = new ServicioEjecucion
                                {
                                    ServicioId                        = nextCfg.Servicio.Id,
                                    ServicioConfiguracionId           = nextCfg.Id,
                                    ServicioEjecucionDesencadenadorId = parentExecId,
                                    FechaEjecucion                    = DateTime.UtcNow,
                                    Estado        = false,
                                    MensajeError  = ex.Message,
                                    Parametros    = System.Text.Json.JsonSerializer.Serialize(nextParams),
                                    Resultado     = null
                                };
                                execErr = await auditoriaService.RegistrarEjecucionAsync(execErr);

                                throw; 
                            }
                        })
                    .SelectMany(tuple =>
                    {
                        var (srv, res, execId) = tuple;
                        return res is IEnumerable<object> list
                            ? list.ToObservable().SelectMany(item =>
                                  ContinueRecursively(
                                      srv,
                                      item,
                                      execId,
                                      new HashSet<string>(visited)))   
                            : ContinueRecursively(srv, res, execId, visited);
                    });
                });
        }
    }
}
