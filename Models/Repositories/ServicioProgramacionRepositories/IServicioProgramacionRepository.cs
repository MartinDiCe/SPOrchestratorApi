using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ServicioProgramacionRepositories
{
    /// <summary>
    /// Define las operaciones CRUD para la entidad <see cref="ServicioProgramacion"/>
    /// de manera reactiva.
    /// </summary>
    public interface IServicioProgramacionRepository
    {
        /// <summary>
        /// Obtiene una programación por su ID.
        /// </summary>
        /// <param name="id">ID de la programación.</param>
        /// <returns>Una secuencia observable con la programación encontrada.</returns>
        IObservable<ServicioProgramacion> GetByIdAsync(int id);

        /// <summary>
        /// Crea una nueva programación.
        /// </summary>
        /// <param name="programacion">Objeto de programación a crear.</param>
        /// <returns>Una secuencia observable con la programación creada.</returns>
        IObservable<ServicioProgramacion> CreateAsync(ServicioProgramacion programacion);

        /// <summary>
        /// Actualiza una programación existente.
        /// </summary>
        /// <param name="programacion">Objeto de programación con los datos actualizados.</param>
        /// <returns>Una secuencia observable con la programación actualizada.</returns>
        IObservable<ServicioProgramacion> UpdateAsync(ServicioProgramacion programacion);

        /// <summary>
        /// Aplica un soft-delete a una programación.
        /// </summary>
        /// <param name="id">ID de la programación a eliminar.</param>
        /// <returns>Una secuencia observable con la programación eliminada.</returns>
        IObservable<ServicioProgramacion> SoftDeleteAsync(int id);

        /// <summary>
        /// Restaura una programación que fue eliminada (soft-delete).
        /// </summary>
        /// <param name="id">ID de la programación a restaurar.</param>
        /// <returns>Una secuencia observable con la programación restaurada.</returns>
        IObservable<ServicioProgramacion> RestoreAsync(int id);

        /// <summary>
        /// Obtiene todas las programaciones que no han sido eliminadas.
        /// </summary>
        /// <returns>Una secuencia observable con la lista de programaciones.</returns>
        IObservable<IList<ServicioProgramacion>> GetAllAsync();

        /// <summary>
        /// Obtiene las programaciones asociadas a una configuración de servicio específica.
        /// </summary>
        /// <param name="servicioConfiguracionId">ID de la configuración de servicio.</param>
        /// <returns>Una secuencia observable con la lista de programaciones asociadas.</returns>
        IObservable<ServicioProgramacion?> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId);

    }
}
