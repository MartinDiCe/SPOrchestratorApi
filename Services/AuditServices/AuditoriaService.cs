using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.AuditServices
{
    public class AuditoriaService(ApplicationDbContext context) : IAuditoriaService
    {
        /// <inheritdoc />
        public async Task<ServicioEjecucion> RegistrarEjecucionAsync(ServicioEjecucion ejecucion)
        {
            context.ServicioEjecucion.Add(ejecucion);
            await context.SaveChangesAsync();
            return ejecucion;  
        }
    }
}