namespace SPOrchestratorAPI.Services.HangFireServices;

/// <summary>
/// Encapsula la lógica de “sincronizar & registrar” los jobs de Hangfire.
/// </summary>
public interface IHangfireJobService
{
    /// <remarks>
    /// 1. Elimina todo lo que ya no esté programado  
    /// 2. Vuelve a registrar los jobs válidos
    /// </remarks>
    void RefreshAllRecurringJobs();
}