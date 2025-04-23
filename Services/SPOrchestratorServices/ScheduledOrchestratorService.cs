using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using SPOrchestratorAPI.Services.ChainOrchestratorServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;

namespace SPOrchestratorAPI.Services.SPOrchestratorServices
{
    public class ScheduledOrchestratorService : IScheduledOrchestratorService
    {
        private readonly IServicioProgramacionService _programacionService;
        private readonly IServicioConfiguracionService _configService;
        private readonly IChainOrchestratorService _chainOrchestrator;
        private readonly ILogger<ScheduledOrchestratorService> _logger;

        public ScheduledOrchestratorService(
            IServicioProgramacionService programacionService,
            IServicioConfiguracionService configService,
            IChainOrchestratorService chainOrchestrator,
            ILogger<ScheduledOrchestratorService> logger)
        {
            _programacionService = programacionService ?? throw new ArgumentNullException(nameof(programacionService));
            _configService       = configService       ?? throw new ArgumentNullException(nameof(configService));
            _chainOrchestrator   = chainOrchestrator   ?? throw new ArgumentNullException(nameof(chainOrchestrator));
            _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invocado por Hangfire. Espera a que todo el pipeline reactivo termine antes de cerrar el scope.
        /// </summary>
        public async Task EjecutarProgramadoAsync(string serviceName, int servicioConfigId)
        {
            _logger.LogInformation(
                "Scheduler (Hangfire) → Invocando {ServiceName} (ConfigId={ConfigId})",
                serviceName, servicioConfigId);

            try
            {
                await EjecutarProgramado(servicioConfigId)
                    .ToTask()
                    .ConfigureAwait(false);

                _logger.LogInformation(
                    "Scheduler (Hangfire) → Finalizado {ServiceName}-{ConfigId}",
                    serviceName, servicioConfigId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Scheduler (Hangfire) → Error en {ServiceName}-{ConfigId}",
                    serviceName, servicioConfigId);
                throw; 
            }
        }

        /// <inheritdoc />
        public IObservable<Unit> EjecutarProgramado(int servicioConfigId)
        {
            return _configService.GetByIdAsync(servicioConfigId)
                .Select(config =>
                {
                    if (config == null)
                        throw new InvalidOperationException($"No existe config con ID {servicioConfigId}");
                    if (!config.EsProgramado)
                        throw new InvalidOperationException($"ConfigId={config.Id} no está programado.");
                    _logger.LogInformation(
                        "Scheduler → config.Id={ConfigId}, Procedimiento='{Name}', Params={Params}",
                        config.Id, config.NombreProcedimiento, config.Parametros);
                    return config;
                })
                .SelectMany(
                    config => _programacionService
                        .GetByServicioConfiguracionIdAsync(config.Id),
                    (config, prog) => (config, prog)
                )
                .SelectMany(tuple =>
                {
                    var (config, prog) = tuple;
                    if (prog == null)
                    {
                        _logger.LogInformation(
                            "Scheduler → sin programación para ConfigId={ConfigId}", config.Id);
                        return Observable.Empty<Unit>();
                    }

                    var now = DateTime.UtcNow;
                    if ((prog.StartDate != default && now < prog.StartDate) ||
                        (prog.EndDate   != default && now > prog.EndDate))
                    {
                        _logger.LogInformation(
                            "Scheduler → fuera de ventana [{Start}–{End}] para ConfigId={ConfigId}",
                            prog.StartDate, prog.EndDate, config.Id);
                        return Observable.Empty<Unit>();
                    }

                    IDictionary<string, object>? parameters = null;
                    if (!string.IsNullOrWhiteSpace(config.Parametros))
                    {
                        try
                        {
                            parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(config.Parametros);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(
                                ex, "Scheduler → JSON inválido en Parametros de ConfigId={ConfigId}", config.Id);
                        }
                    }

                    _logger.LogInformation("Scheduler → disparando Chain para ConfigId={ConfigId}", config.Id);

                    return _chainOrchestrator
                        .EjecutarConContinuacionAsync(config.Servicio?.Name, parameters)
                        .Do(
                            _  => _logger.LogDebug("Scheduler → chunk recibido para ConfigId={ConfigId}", config.Id),
                            ex => _logger.LogError(ex,     "Scheduler → fallo en ConfigId={ConfigId}", config.Id),
                            () => _logger.LogInformation("Scheduler → finalizado para ConfigId={ConfigId}", config.Id)
                        )
                        .Select(_ => Unit.Default);
                })
                .DefaultIfEmpty(Unit.Default)
                .Catch<Unit, Exception>(ex =>
                {
                    _logger.LogError(ex, "Scheduler → error global en ConfigId={ConfigId}", servicioConfigId);
                    return Observable.Throw<Unit>(ex);
                });
        }
    }
}
