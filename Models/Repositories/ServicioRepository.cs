using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories;

public class ServicioRepository : RepositoryBase<Servicio>
{
    public ServicioRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Obtiene todos los servicios activos de manera reactiva.
    /// </summary>
    public IObservable<IEnumerable<Servicio>> GetActiveServicesAsync()
    {
        return Observable.FromAsync(async () =>
            await _context.Set<Servicio>()
                .Where(s => s.Status == true && s.Deleted == false)
                .ToListAsync());
    }
}