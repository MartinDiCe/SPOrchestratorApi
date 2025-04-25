using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;
using SPOrchestratorAPI.Services.SPOrchestratorServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;

namespace SPOrchestratorAPI.Services.HangFireServices
{
    /// <summary>
    /// Implementación de <see cref="IHangfireJobService"/>, se encarga de
    /// borrar y re-registrar jobs con prefijo "Orquestador-".
    /// </summary>
    public sealed class HangfireJobService(
        IServiceProvider rootProvider,
        IRecurringJobRegistrar registrar,
        ILogger<HangfireJobService> logger) : IHangfireJobService
    {
        public void RefreshAllRecurringJobs()
        {
           
            HangfireJobsInitializer
                .CleanUnscheduledJobsAsync(
                    rootProvider,
                    logger)
                .GetAwaiter().GetResult();

            // 2) registro / actualización
            registrar.RegisterAllJobs();

            logger.LogInformation("✔️ Jobs sincronizados correctamente.");
        }
    }
}
