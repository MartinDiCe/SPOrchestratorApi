using Hangfire;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;
using SPOrchestratorAPI.Services.SPOrchestratorServices;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace SPOrchestratorAPI.Services.HangFireServices
{
    public class RecurringJobRegistrar : IRecurringJobRegistrar
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<RecurringJobRegistrar> _logger;

        public RecurringJobRegistrar(IServiceScopeFactory scopes,
                                     ILogger<RecurringJobRegistrar> logger)
        {
            _scopes = scopes;
            _logger = logger;
        }

        public void RegisterAllJobs()
        {
            _logger.LogInformation("=== Registrando todos los recurring jobs ===");

            using var scope = _scopes.CreateScope();
            var configSvc = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
            var progSvc   = scope.ServiceProvider.GetRequiredService<IServicioProgramacionService>();

            var configs = configSvc
                .GetAllAsync()
                .FirstAsync().ToTask().Result
                .Where(c => c.EsProgramado);

            foreach (var cfg in configs)
            {
                var prog = progSvc
                    .GetByServicioConfiguracionIdAsync(cfg.Id)
                    .FirstAsync().ToTask().Result;

                if (prog == null || string.IsNullOrWhiteSpace(prog.CronExpression))
                    continue;

                // Saco el nombre real del Servicio para pasar a Hangfire
                var serviceName = cfg.Servicio?.Name
                    ?? throw new InvalidOperationException(
                        $"La configuración {cfg.Id} no tiene Servicio asociado.");

                var jobId = $"Orquestador-{serviceName}-{cfg.Id}";

                RecurringJob.AddOrUpdate<IScheduledOrchestratorService>(
                    jobId,
                    sched => sched.EjecutarProgramadoAsync(serviceName, cfg.Id),
                    prog.CronExpression,
                    TimeZoneInfo.Local
                );

                _logger.LogInformation("→ Registrado {JobId} ({Cron})",
                                       jobId, prog.CronExpression);
            }
        }
    }
}
