using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;
using SPOrchestratorAPI.Services.SPOrchestratorServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;

namespace SPOrchestratorAPI.Services.HangFireServices
{
    /// <summary>
    /// Implementación de <see cref="IHangfireJobService"/>, se encarga de
    /// borrar y re-registrar jobs con prefijo "Orquestador-".
    /// </summary>
    public class HangfireJobService(
        IServiceScopeFactory scopeFactory,
        ILogger<HangfireJobService> logger)
        : IHangfireJobService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        private readonly ILogger<HangfireJobService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void RefreshAllRecurringJobs()
        {
            _logger.LogInformation("Iniciando refresco de recurring jobs...");

            var recurringJobs = JobStorage.Current
                .GetConnection()
                .GetRecurringJobs()
                .Where(j => j.Id.StartsWith("Orquestador-"));
            var manager = new RecurringJobManager();
            foreach (var job in recurringJobs)
            {
                manager.RemoveIfExists(job.Id);
                _logger.LogInformation("Removido job {JobId}", job.Id);
            }

            using var scope = _scopeFactory.CreateScope();
            var configService      = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
            var programacionService = scope.ServiceProvider.GetRequiredService<IServicioProgramacionService>();
            var scheduler           = scope.ServiceProvider.GetRequiredService<IScheduledOrchestratorService>();
            
            var configs = configService
                .GetAllAsync()
                .FirstAsync()
                .ToTask()
                .Result;

            foreach (var cfg in configs)
            {
                if (!cfg.EsProgramado)
                {
                    _logger.LogDebug("ConfigId={ConfigId} no está en modo programado, se omite.", cfg.Id);
                    continue;
                }

                _logger.LogInformation("Procesando ConfigId={ConfigId}, SP=" + cfg.NombreProcedimiento, cfg.Id);

                var prog = programacionService
                    .GetByServicioConfiguracionIdAsync(cfg.Id)
                    .FirstAsync()
                    .ToTask()
                    .Result;

                if (prog == null)
                {
                    _logger.LogWarning("No existe programación para ConfigId={ConfigId}.", cfg.Id);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(prog.CronExpression))
                {
                    _logger.LogWarning("ConfigId={ConfigId} no tiene CronExpression válido.", cfg.Id);
                    continue;
                }

                var jobId = $"Orquestador-{cfg.Id}";
                RecurringJob.AddOrUpdate(
                    jobId,
                    () => scheduler.EjecutarProgramado(cfg.Id),
                    prog.CronExpression,
                    TimeZoneInfo.Local
                );

                _logger.LogInformation("Registrado job {JobId} → {Cron}", jobId, prog.CronExpression);
                
            }
        }
    }
}
