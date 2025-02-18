using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ContinueWithRepositories
{
    /// <summary>
    /// Define las operaciones para gestionar la entidad ServicioContinueWith.
    /// </summary>
    public interface IServicioContinueWithRepository
    {
        /// <summary>
        /// Obtiene un registro de ServicioContinueWith por su ID.
        /// </summary>
        IObservable<ServicioContinueWith> GetByIdAsync(int id);

        /// <summary>
        /// Crea un nuevo registro de ServicioContinueWith.
        /// </summary>
        IObservable<ServicioContinueWith> CreateAsync(ServicioContinueWith entity);

        /// <summary>
        /// Actualiza un registro de ServicioContinueWith existente.
        /// </summary>
        IObservable<ServicioContinueWith> UpdateAsync(ServicioContinueWith entity);

        /// <summary>
        /// Marca lógicamente un registro de ServicioContinueWith como eliminado.
        /// </summary>
        IObservable<ServicioContinueWith> SoftDeleteAsync(int id);

        /// <summary>
        /// Restaura un registro previamente marcado como eliminado.
        /// </summary>
        IObservable<ServicioContinueWith> RestoreAsync(int id);

        /// <summary>
        /// Obtiene todos los registros de ServicioContinueWith no eliminados.
        /// </summary>
        IObservable<IList<ServicioContinueWith>> GetAllAsync();

        /// <summary>
        /// Obtiene todos los registros de ServicioContinueWith asociados a una configuración específica.
        /// </summary>
        IObservable<IList<ServicioContinueWith>> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId);
    }
}