namespace SPOrchestratorAPI.Services.HangFireServices;

/// <summary>
/// Interfaz para la gestión de recurring jobs de Hangfire,
/// permite refrescar el registro de jobs según la tabla de programación.
/// </summary>
public interface IHangfireJobService
{
    /// <summary>
    /// Elimina y registra nuevamente todos los recurring jobs
    /// basados en ServicioProgramacion.
    /// </summary>
    void RefreshAllRecurringJobs();
    
}