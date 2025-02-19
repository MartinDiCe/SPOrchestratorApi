using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.AuditServices
{
    public class AuditoriaService(ApplicationDbContext context) : IAuditoriaService
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <inheritdoc />
        public async Task RegistrarEjecucionAsync(ServicioEjecucion ejecucion)
        {
            if (ejecucion == null)
                throw new ArgumentNullException(nameof(ejecucion));
            
            _context.ServicioEjecucion.Add(ejecucion);
            await _context.SaveChangesAsync();
        }
    }
}