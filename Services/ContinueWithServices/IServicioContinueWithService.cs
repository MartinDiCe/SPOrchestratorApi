using SPOrchestratorAPI.Models.DTOs.ContinueWithDtos;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.ContinueWithServices
{
    /// <summary>
    /// Define las operaciones para gestionar los mapeos de continuación de procesos.
    /// Estos mapeos permiten definir cómo se deben transferir o transformar
    /// los campos del resultado de un proceso inicial hacia los parámetros de un proceso de continuación.
    /// Las operaciones se implementan de forma reactiva, facilitando la composición y manejo asíncrono.
    /// </summary>
    public interface IServicioContinueWithService
    {
        /// <summary>
        /// Crea un nuevo mapeo de continuación.
        /// Se valida que el servicio de continuación exista y que la cadena de mapeo tenga el formato correcto.
        /// </summary>
        /// <param name="entity">DTO con los datos necesarios para crear el mapeo de continuación.</param>
        /// <returns>Un observable que retorna el mapeo de continuación creado.</returns>
        IObservable<ServicioContinueWith> CreateAsync(CreateServicioContinueWithDto entity);

        /// <summary>
        /// Actualiza un mapeo de continuación existente.
        /// Se valida la existencia del servicio de continuación y se verifica que la cadena de mapeo cumpla con el formato esperado.
        /// </summary>
        /// <param name="entity">DTO con los datos necesarios para actualizar el mapeo.</param>
        /// <returns>Un observable que retorna el mapeo de continuación actualizado.</returns>
        IObservable<ServicioContinueWith> UpdateAsync(UpdateServicioContinueWithDto entity);

        /// <summary>
        /// Obtiene un mapeo de continuación a partir de su identificador único.
        /// </summary>
        /// <param name="id">Identificador del mapeo.</param>
        /// <returns>Un observable que retorna el mapeo encontrado.</returns>
        IObservable<ServicioContinueWith> GetByIdAsync(int id);

        /// <summary>
        /// Marca lógicamente (soft delete) un mapeo de continuación, sin eliminar físicamente el registro.
        /// </summary>
        /// <param name="id">Identificador del mapeo a eliminar.</param>
        /// <returns>Un observable que retorna el mapeo luego de haber sido marcado como eliminado.</returns>
        IObservable<ServicioContinueWith> SoftDeleteAsync(int id);

        /// <summary>
        /// Restaura un mapeo de continuación previamente eliminado (soft delete).
        /// </summary>
        /// <param name="id">Identificador del mapeo a restaurar.</param>
        /// <returns>Un observable que retorna el mapeo restaurado.</returns>
        IObservable<ServicioContinueWith> RestoreAsync(int id);

        /// <summary>
        /// Obtiene todos los mapeos de continuación que no han sido eliminados lógicamente.
        /// </summary>
        /// <returns>Un observable que retorna una lista con todos los mapeos no eliminados.</returns>
        IObservable<IList<ServicioContinueWith>> GetAllAsync();

        /// <summary>
        /// Obtiene los mapeos de continuación asociados a una configuración de servicio inicial específica.
        /// </summary>
        /// <param name="servicioConfiguracionId">Identificador de la configuración del servicio inicial.</param>
        /// <returns>Un observable que retorna una lista con los mapeos asociados a dicha configuración.</returns>
        IObservable<IList<ServicioContinueWith>> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId);
    }
}