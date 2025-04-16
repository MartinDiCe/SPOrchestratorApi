using System.Reactive;
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;

namespace SPOrchestratorAPI.Services.SPOrchestratorServices
{
    public class ScheduledOrchestratorService(
        IServicioProgramacionService programacionService,
        IServicioConfiguracionService configService,
        ISpOrchestratorService spOrchestrator,
        ILogger<ScheduledOrchestratorService> logger)
        : IScheduledOrchestratorService
    {
        private readonly IServicioProgramacionService _programacionService 
            = programacionService ?? throw new ArgumentNullException(nameof(programacionService));
        private readonly IServicioConfiguracionService _configService 
            = configService ?? throw new ArgumentNullException(nameof(configService));
        private readonly ISpOrchestratorService _spOrchestrator 
            = spOrchestrator ?? throw new ArgumentNullException(nameof(spOrchestrator));
        private readonly ILogger<ScheduledOrchestratorService> _logger 
            = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public IObservable<Unit> EjecutarProgramado(int servicioConfigId)
        {
            
            return _configService.GetByIdAsync(servicioConfigId)
                
                .SelectMany<ServicioConfiguracion, (ServicioConfiguracion, ServicioProgramacion?)>(config =>
                {
                    if (config == null)
                    {
                        _logger.LogError("No existe la configuración con Id={ConfigId}.", servicioConfigId);
                        return Observable.Throw<(ServicioConfiguracion, ServicioProgramacion?)>(
                            new InvalidOperationException($"No existe config con ID {servicioConfigId}")
                        );
                    }

                    if (!config.EsProgramado)
                    {
                        _logger.LogWarning("La configuración Id={ConfigId} ya no está programada. Se omite.", config.Id);
                        return Observable.Throw<(ServicioConfiguracion, ServicioProgramacion?)>(
                            new InvalidOperationException($"ConfigId={config.Id} no está en modo programado.")
                        );
                    }

                    _logger.LogInformation(
                        "Cargada config (Id={0}), NombreProcedimiento={1}, verificando programación...",
                        config.Id, config.NombreProcedimiento
                    );
                    
                    return _programacionService.GetByServicioConfiguracionIdAsync(config.Id)
                        
                        .Select(prog => (config, prog));
                })
                
                .SelectMany<(ServicioConfiguracion, ServicioProgramacion?), Unit>(tuple =>
                {
                    var (config, prog) = tuple;

                    if (prog == null)
                    {
                        _logger.LogInformation("No existe programacion para ConfigId={0}. Se omite ejecución.", config.Id);
                        return Observable.Empty<Unit>();
                        
                    }

                    var now = DateTime.UtcNow;

                    if (prog.StartDate != default && now < prog.StartDate)
                    {
                        _logger.LogInformation(
                            "Todavía no llega StartDate={0} para ConfigId={1}. Se omite ejecución.",
                            prog.StartDate, config.Id
                        );
                        // No hay error: simplemente no se ejecuta => Observable.Empty<Unit>()
                        return Observable.Empty<Unit>();
                    }

                    if (prog.EndDate != default && now > prog.EndDate)
                    {
                        _logger.LogInformation(
                            "La configuración {0} expiró en {1}. Se omite la ejecución.",
                            config.Id, prog.EndDate
                        );
                        return Observable.Empty<Unit>();
                    }

                    // 3) Todo OK: llamar a la ejecución real
                    _logger.LogInformation(
                        "Ejecutando ConfigId={0}, NombreProcedimiento={1}...",
                        config.Id, config.NombreProcedimiento
                    );

                    return _spOrchestrator
                        .EjecutarPorNombreAsync(config.NombreProcedimiento)
                        .Do(
                            // onNext
                            resultado => {
                                _logger.LogDebug("Recibido chunk de resultado para ConfigId={0}", config.Id);
                            },
                            // onError
                            ex => {
                                _logger.LogError(ex, "Error al ejecutar {0}", config.NombreProcedimiento);
                            },
                            // onCompleted
                            () => {
                                _logger.LogInformation("Ejecución completada para ConfigId={0}", config.Id);
                            }
                        )
                        .Select(_ => Unit.Default);
                })
                .DefaultIfEmpty(Unit.Default)
                .Catch<Unit, Exception>(ex =>
                {
                    _logger.LogError(
                        ex, "Falla global al ejecutar programado para ConfigId={0}.", 
                        servicioConfigId
                    );
                    return Observable.Throw<Unit>(ex);
                });
        }
    }
}