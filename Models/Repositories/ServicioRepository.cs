using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories;

public class ServicioRepository : RepositoryBase<Servicio>
{
    public ServicioRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Servicio>> GetActiveServicesAsync()
    {
        return await _context.Set<Servicio>()
            .Where(s => s.Status == true)
            .ToListAsync();
    }
}