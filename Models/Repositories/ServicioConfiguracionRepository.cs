using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.Logging;

namespace SPOrchestratorAPI.Models.Repositories
{
    /// <summary>
    /// Repositorio específico para `ServicioConfiguracion`, heredando `RepositoryBase&lt; Servicio&gt; `.
    /// </summary>
    public class ServicioConfiguracionRepository(
        ApplicationDbContext context,
        ILoggerService<RepositoryBase<ServicioConfiguracion>> logger)
        : RepositoryBase<ServicioConfiguracion>(context, logger)
    {
        // Se pasa el logger de tipo RepositoryBase<ServicioConfiguracion>

        /// <summary>
        /// Obtiene la configuración de un servicio basado en su ID de manera reactiva.
        /// </summary>
        public IObservable<ServicioConfiguracion?> GetByServicioIdAsync(int servicioId)
        {
            return Observable.FromAsync(() =>
            {
                try
                {
                    return Context.Set<ServicioConfiguracion>()
                        .FirstOrDefaultAsync(sc => sc.ServicioId == servicioId && !sc.Deleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al obtener la configuración del servicio con ID {servicioId}: {ex.Message}", ex);
                    throw new Exception($"Error al obtener la configuración del servicio con ID {servicioId}.", ex);
                }
            });
        }
    }
}