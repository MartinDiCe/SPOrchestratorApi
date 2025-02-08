using SPOrchestratorAPI.Models.DTOs.ServicioConfiguracionDtos;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.ServicioConfiguracionServices
{
    /// <summary>
    /// Define la lógica de negocio para la entidad <see cref="ServicioConfiguracion"/>.
    /// </summary>
    public interface IServicioConfiguracionService
    {
        /// <summary>
        /// Obtiene una configuración por su identificador (ID), excluyendo las eliminadas.
        /// Lanza una excepción si no existe.
        /// </summary>
        IObservable<ServicioConfiguracion> GetByIdAsync(int id);

        /// <summary>
        /// Crea una nueva configuración de servicio a partir del DTO.
        /// </summary>
        IObservable<ServicioConfiguracion> CreateAsync(CreateServicioConfiguracionDto dto);

        /// <summary>
        /// Actualiza una configuración existente a partir del DTO.
        /// </summary>
        IObservable<ServicioConfiguracion> UpdateAsync(UpdateServicioConfiguracionDto dto);

        /// <summary>
        /// Marca una configuración como eliminada (soft delete).
        /// </summary>
        IObservable<ServicioConfiguracion> SoftDeleteAsync(int id);

        /// <summary>
        /// Restaura una configuración que fue previamente marcada como eliminada.
        /// </summary>
        IObservable<ServicioConfiguracion> RestoreAsync(int id);

        /// <summary>
        /// Obtiene todas las configuraciones que no estén eliminadas.
        /// </summary>
        IObservable<IList<ServicioConfiguracion>> GetAllAsync();

        /// <summary>
        /// Obtiene las configuraciones asociadas a un <see cref="Servicio"/> específico (por ID),
        /// excluyendo configuraciones eliminadas.
        /// </summary>
        IObservable<IList<ServicioConfiguracion>> GetByServicioIdAsync(int servicioId);
    }
}