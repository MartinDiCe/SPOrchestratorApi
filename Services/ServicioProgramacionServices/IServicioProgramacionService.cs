using SPOrchestratorAPI.Models.DTOs.ServicioProgramacioDtos;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.ServicioProgramacionServices
{
    /// <summary>
    /// Define las operaciones de negocio para la entidad <see cref="ServicioProgramacion"/>.
    /// </summary>
    public interface IServicioProgramacionService
    {
        /// <summary>
        /// Crea una nueva programación de servicio validando la expresión CRON.
        /// </summary>
        /// <param name="dto">Datos para crear la programación.</param>
        /// <returns>Una secuencia observable con la programación creada.</returns>
        IObservable<ServicioProgramacion> CreateAsync(CreateServicioProgramacionDto dto);

        /// <summary>
        /// Actualiza una programación existente validando la expresión CRON.
        /// </summary>
        /// <param name="dto">Datos actualizados de la programación.</param>
        /// <returns>Una secuencia observable con la programación actualizada.</returns>
        IObservable<ServicioProgramacion> UpdateAsync(UpdateServicioProgramacionDto dto);

        /// <summary>
        /// Obtiene una programación por su ID.
        /// </summary>
        IObservable<ServicioProgramacion> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las programaciones no eliminadas.
        /// </summary>
        IObservable<IList<ServicioProgramacion>> GetAllAsync();

        /// <summary>
        /// Obtiene las programaciones asociadas a una configuración de servicio específica.
        /// </summary>
        IObservable<ServicioProgramacion?> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId);

        /// <summary>
        /// Aplica un soft-delete a una programación.
        /// </summary>
        IObservable<ServicioProgramacion> SoftDeleteAsync(int id);

        /// <summary>
        /// Restaura una programación que fue eliminada (soft-delete).
        /// </summary>
        IObservable<ServicioProgramacion> RestoreAsync(int id);
    }
}
