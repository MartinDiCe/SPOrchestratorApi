using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.DTOs;

namespace SPOrchestratorAPI.Services
{
    public interface IServicioService
    {
        /// <summary>
        /// Crea un nuevo servicio si no existe otro con el mismo nombre (no eliminado).
        /// </summary>
        IObservable<Servicio> CreateAsync(CreateServicioDto dto);

        /// <summary>
        /// Obtiene un servicio por su identificador (lanza excepción si no existe).
        /// </summary>
        IObservable<Servicio> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene un servicio por nombre (lanza excepción si no existe).
        /// </summary>
        IObservable<Servicio> GetByNameAsync(string name);
        
        /// <summary>
        /// Retorna todos los servicios, incluidos los inactivos.
        /// </summary>
        IObservable<IList<Servicio>> GetAllAsync();

        /// <summary>
        /// Retorna todos los servicios activos (Status = true y no eliminados).
        /// </summary>
        IObservable<IList<Servicio>> GetActiveServicesAsync();

        /// <summary>
        /// Elimina lógicamente un servicio (soft delete).
        /// </summary>
        IObservable<Servicio> SoftDeleteAsync(int id);

        /// <summary>
        /// Restaura un servicio previamente marcado como eliminado.
        /// </summary>
        IObservable<Servicio> RestoreAsync(int id);

        /// <summary>
        /// Inactiva un servicio, estableciendo <c>Status = false</c>.
        /// </summary>
        IObservable<Servicio> DeactivateAsync(int id);

        /// <summary>
        /// Activa un servicio, estableciendo <c>Status = true</c>.
        /// </summary>
        IObservable<Servicio> ActivateAsync(int id);

        /// <summary>
        /// Actualiza un servicio existente con los campos indicados.
        /// </summary>
        /// <param name="dto">DTO con los campos a actualizar (incluye el Id).</param>
        IObservable<Servicio> UpdateAsync(UpdateServicioDto dto);
    }
}