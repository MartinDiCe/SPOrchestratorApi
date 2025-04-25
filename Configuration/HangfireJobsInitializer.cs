using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using System.Reactive.Linq;

namespace SPOrchestratorAPI.Configuration;

/// <summary>
///     Utilidades de limpieza y sincronización para los artefactos de Hangfire
///     (<c>recurring-jobs</c>, jobs encolados, planificados o en ejecución).
/// </summary>
public static class HangfireJobsInitializer
{
    private const string PREFIX = "Orquestador-";

    /// <summary>
    /// Elimina de Hangfire cualquier job asociado a un <c>cfgId</c> cuyo
    /// <c>EsProgramado</c> sea <see langword="false"/>.
    /// </summary>
    public static async Task CleanUnscheduledJobsAsync(
        IServiceProvider sp,
        ILogger logger)
    {
        using var scope   = sp.CreateScope();
        var cfgSvc        = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();

        var monitor    = JobStorage.Current.GetMonitoringApi();
        var connection = JobStorage.Current.GetConnection();   // ← para recurring
        
        var cfgActivas = (await cfgSvc.GetAllAsync().FirstAsync())
                         .Where(c => c.EsProgramado)
                         .Select(c => c.Id)
                         .ToHashSet();
        
        foreach (var rec in connection.GetRecurringJobs())
        {
            var id = rec.Id;                                   
            if (!id.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase))
                continue;

            var segs = id.Split('-');
            if (!int.TryParse(segs[^1], out var cfgId) || cfgActivas.Contains(cfgId))
                continue;                                       // sigue válido → no tocar

            RecurringJob.RemoveIfExists(id);
            logger.LogInformation("🗑 RecurringJob '{Id}' eliminado (cfgId={Cfg})", id, cfgId);
        }
        
        CleanList(monitor.ScheduledJobs (0, int.MaxValue), cfgActivas, logger);
        CleanList(monitor.ProcessingJobs(0, int.MaxValue), cfgActivas, logger);

        foreach (var q in monitor.Queues())
        {
            var perPage = q.Length > int.MaxValue ? int.MaxValue : (int)q.Length;
            CleanList(monitor.EnqueuedJobs(q.Name, 0, perPage), cfgActivas, logger);
        }
    }
    
    private static void CleanList<TDto>(
        IEnumerable<KeyValuePair<string, TDto>> lote,
        HashSet<int> cfgActivas,
        ILogger log)
    {
        foreach (var (jobId, dto) in lote)
        {
            dynamic d   = dto!;
            var     job = d.Job as Job;

            if (job?.Args?.Count < 2) continue;

            if (job.Args[1] is int cfgId && !cfgActivas.Contains(cfgId))
            {
                BackgroundJob.Delete(jobId);
                log.LogInformation("🗑 Job {Id} eliminado (cfgId={Cfg})", jobId, cfgId);
            }
        }
    }
}
