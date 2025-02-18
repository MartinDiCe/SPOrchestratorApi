using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.AuditServices
{
    public interface IAuditoriaService
    {
        /// <summary>
        /// Registra la ejecución de un proceso en la base de datos.
        /// </summary>
        /// <param name="ejecucion">La información de la ejecución.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task RegistrarEjecucionAsync(ServicioEjecucion ejecucion);
    }
}