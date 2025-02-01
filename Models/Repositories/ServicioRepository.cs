using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories;

/// <summary>
/// Repositorio específico para `Servicio`, heredando `RepositoryBase&lt; Servicio&gt; `.
/// </summary>
public class ServicioRepository(ApplicationDbContext context) : RepositoryBase<Servicio>(context)
{
    /// <summary>
    /// Obtiene todos los servicios activos de manera reactiva.
    /// </summary>
    /// <returns>
    /// Un flujo observable que emite una colección de servicios activos, es decir, aquellos cuyo estado es `true` y que no han sido eliminados (`Deleted == false`).
    /// </returns>
    public IObservable<IEnumerable<Servicio>> GetActiveServicesAsync()
    {
        return Observable.FromAsync(async () =>
            await Context.Set<Servicio>()  // 🔹 Usa 'Context' en lugar de '_context'
                .Where(s => s.Status == true && s.Deleted == false)
                .ToListAsync());
    }
}