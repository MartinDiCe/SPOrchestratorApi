using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories;

public class ServicioRepository : RepositoryBase<Servicio>
{
    public ServicioRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Obtiene todos los servicios activos (que no están eliminados).
    /// </summary>
    public async Task<IEnumerable<Servicio>> GetActiveServicesAsync()
    {
        return await _context.Set<Servicio>()
            .Where(s => s.Status == true && s.Deleted == false)
            .ToListAsync();
    }
}