using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories;

/// <summary>
/// Repositorio específico para `ServicioConfiguracion`, heredando `RepositoryBase&lt; ServicioConfiguracion&gt; `.
/// </summary>
public class ServicioConfiguracionRepository(ApplicationDbContext context)
    : RepositoryBase<ServicioConfiguracion>(context)
{
    /// <summary>
    /// Obtiene la configuración de un servicio basado en su ID.
    /// </summary>
    public IObservable<ServicioConfiguracion?> GetByServicioIdAsync(int servicioId)
    {
        return Observable.FromAsync(() =>
            context.Set<ServicioConfiguracion>()
                .FirstOrDefaultAsync(sc => sc.ServicioId == servicioId && !sc.Deleted));
    }
}