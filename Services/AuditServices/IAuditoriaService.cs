using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.AuditServices
{
    public interface IAuditoriaService
    {
        /// <summary>
        /// Registra la ejecución de un proceso en la base de datos y devuelve
        /// la entidad con el Id asignado.
        /// </summary>
        Task<ServicioEjecucion> RegistrarEjecucionAsync(ServicioEjecucion ejecucion);
    }
}