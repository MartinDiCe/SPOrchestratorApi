namespace SPOrchestratorAPI.Services.HangFireServices;

public interface IRecurringJobRegistrar
{
    /// <summary>
    /// Registra en Hangfire todos los recurring jobs (incluido el refresco automático).
    /// </summary>
    void RegisterAllJobs();
}