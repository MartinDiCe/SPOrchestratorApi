using System.Reactive;
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ChainOrchestratorServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;

namespace SPOrchestratorAPI.Services.SPOrchestratorServices
{
    public class ScheduledOrchestratorService(
        IServicioProgramacionService programacionService,
        IServicioConfiguracionService configService,
        IChainOrchestratorService chainOrchestrator,            
        ILogger<ScheduledOrchestratorService> logger)
        : IScheduledOrchestratorService
    {
        private readonly IServicioProgramacionService _programacionService = programacionService ?? throw new ArgumentNullException(nameof(programacionService));
        private readonly IServicioConfiguracionService _configService       = configService       ?? throw new ArgumentNullException(nameof(configService));
        private readonly IChainOrchestratorService _chainOrchestrator       = chainOrchestrator   ?? throw new ArgumentNullException(nameof(chainOrchestrator));
        private readonly ILogger<ScheduledOrchestratorService> _logger      = logger             ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public IObservable<Unit> EjecutarProgramado(int servicioConfigId)
        {
            return _configService.GetByIdAsync(servicioConfigId)
                .SelectMany<ServicioConfiguracion, (ServicioConfiguracion, ServicioProgramacion?)>(config =>
                {
                    if (config == null)
                        return Observable.Throw<(ServicioConfiguracion, ServicioProgramacion?)>(
                            new InvalidOperationException($"No existe config con ID {servicioConfigId}"));

                    if (!config.EsProgramado)
                        return Observable.Throw<(ServicioConfiguracion, ServicioProgramacion?)>(
                            new InvalidOperationException($"ConfigId={config.Id} no está programado."));

                    _logger.LogInformation(
                        "Scheduler → config.Id={ConfigId}, SP/Vista/Endpoint={Name}",
                        config.Id, config.NombreProcedimiento);

                    return _programacionService
                        .GetByServicioConfiguracionIdAsync(config.Id)
                        .Select(prog => (config, prog));
                })
                .SelectMany<(ServicioConfiguracion, ServicioProgramacion?), Unit>(tuple =>
                {
                    var (config, prog) = tuple;

                    if (prog == null)
                    {
                        _logger.LogInformation("Scheduler → sin programación en ConfigId={0}", config.Id);
                        return Observable.Empty<Unit>();
                    }

                    var now = DateTime.UtcNow;
                    if (prog.StartDate   != default && now < prog.StartDate ||
                        prog.EndDate     != default && now > prog.EndDate)
                    {
                        _logger.LogInformation(
                            "Scheduler → fuera de ventana [{0}–{1}] para ConfigId={2}",
                            prog.StartDate, prog.EndDate, config.Id);
                        return Observable.Empty<Unit>();
                    }

                    _logger.LogInformation("Scheduler → disparando Orquestación para ConfigId={0}", config.Id);

                    // Aquí es donde elegimos el chain:
                    return _chainOrchestrator
                        .EjecutarConContinuacionAsync(config.NombreProcedimiento, null)
                        .Do(
                            _   => _logger.LogDebug("Scheduler → chunk recibido para ConfigId={0}", config.Id),
                            ex  => _logger.LogError(ex, "Scheduler → fallo en ConfigId={0}", config.Id),
                            ()  => _logger.LogInformation("Scheduler → finalizado para ConfigId={0}", config.Id)
                        )
                        .Select(_ => Unit.Default);
                })
                .DefaultIfEmpty(Unit.Default)
                .Catch<Unit, Exception>(ex =>
                {
                    _logger.LogError(ex, "Scheduler → error global en ConfigId={0}", servicioConfigId);
                    return Observable.Throw<Unit>(ex);
                });
        }
    }
}
