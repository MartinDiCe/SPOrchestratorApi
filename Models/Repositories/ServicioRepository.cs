using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.Logging;

namespace SPOrchestratorAPI.Models.Repositories
{
    /// <summary>
    /// Repositorio específico para `Servicio`, heredando `RepositoryBase&lt; Servicio&gt; `.
    /// </summary>
    public class ServicioRepository(ApplicationDbContext context, ILoggerService<RepositoryBase<Servicio>> logger)
        : RepositoryBase<Servicio>(context, logger)
    {

        /// <summary>
        /// Obtiene todos los servicios activos de manera reactiva.
        /// </summary>
        public IObservable<IEnumerable<Servicio>> GetActiveServicesAsync()
        {
            return Observable.FromAsync(async () =>
            {
                try
                {
                    return await _dbSet
                        .Where(s => s.Status == true && s.Deleted == false)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al obtener los servicios activos: {ex.Message}", ex);
                    throw new Exception("Error al obtener los servicios activos.");
                }
            });
        }

        /// <summary>
        /// Obtiene un servicio por su nombre de manera reactiva.
        /// </summary>
        public IObservable<Servicio?> GetByNameAsync(string name)
        {
            return Observable.FromAsync(async () =>
            {
                try
                {
                    return await _dbSet.FirstOrDefaultAsync(s => s.Name == name);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al obtener el servicio con nombre {name}: {ex.Message}", ex);
                    throw new Exception($"Error al obtener el servicio con nombre {name}.");
                }
            });
        }
    }
}