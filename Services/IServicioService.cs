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
    }
}