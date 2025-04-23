namespace SPOrchestratorAPI.Services.HangFireServices
{
    /// <summary>
    /// Registra en Hangfire todos los recurring jobs (incluido el refresco automático).
    /// </summary>
    public interface IRecurringJobRegistrar
    {
        void RegisterAllJobs();
    }
}