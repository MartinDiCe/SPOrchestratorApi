using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Hangfire;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;
using SPOrchestratorAPI.Services.SPOrchestratorServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;

namespace SPOrchestratorAPI.Services.HangFireServices
{

    /// <summary>
    /// Implementación de <see cref="IRecurringJobRegistrar"/>, se encarga de
    /// borrar y re-registrar jobs con prefijo "Orquestador-".
    /// </summary>
    public class RecurringJobRegistrar(
        IServiceScopeFactory scopes,
        ILogger<RecurringJobRegistrar> logger)
        : IRecurringJobRegistrar
    {
        private readonly IServiceScopeFactory _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
        private readonly ILogger<RecurringJobRegistrar> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void RegisterAllJobs()
        {
            _logger.LogInformation("=== Registrando todos los recurring jobs ===");

            using (var scope = _scopes.CreateScope())
            {
                var paramRepo = scope.ServiceProvider.GetRequiredService<IParameterRepository>();
                
                var cronParam = paramRepo
                    .GetByNameAsync("JobsRefreshCron")
                    .GetAwaiter()
                    .GetResult()
                    ?.ParameterValue
                    ?? "0 */3 * * *";  

                RecurringJob.AddOrUpdate(
                    "HangfireJobRefresher",
                    () => scope.ServiceProvider
                        .GetRequiredService<IRecurringJobRegistrar>()
                        .RegisterAllJobs(),
                    cronParam,
                    TimeZoneInfo.Local
                );
                _logger.LogInformation(" → Job de refresco registrado (CRON={Cron})", cronParam);
            }

            using (var scope = _scopes.CreateScope())
            {
                var configSvc = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
                var progSvc = scope.ServiceProvider.GetRequiredService<IServicioProgramacionService>();
                var scheduler = scope.ServiceProvider.GetRequiredService<IScheduledOrchestratorService>();

                // Leemos todas las configuraciones programadas
                var configs = configSvc
                    .GetAllAsync()
                    .FirstAsync()
                    .ToTask()
                    .GetAwaiter()
                    .GetResult()
                    .Where(c => c.EsProgramado);

                foreach (var cfg in configs)
                {
                    // Bloqueamos la llamada a la programación
                    var prog = progSvc
                        .GetByServicioConfiguracionIdAsync(cfg.Id)
                        .FirstAsync()
                        .ToTask()
                        .GetAwaiter()
                        .GetResult();

                    if (prog == null || string.IsNullOrWhiteSpace(prog.CronExpression))
                    {
                        _logger.LogWarning("ConfigId={ConfigId} sin prog/cron → omitido", cfg.Id);
                        continue;
                    }

                    var jobId = $"Orquestador-{cfg.Id}";
                    RecurringJob.AddOrUpdate(
                        jobId,
                        () => scheduler.EjecutarProgramado(cfg.Id),
                        prog.CronExpression,
                        TimeZoneInfo.Local
                    );
                    _logger.LogInformation(" → Registrado {JobId} ({Cron})", jobId, prog.CronExpression);
                }
            }
        }
    }
}