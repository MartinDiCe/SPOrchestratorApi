using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories
{
    /// <summary>
    /// Define las operaciones de acceso a datos para la entidad <see cref="Servicio"/> de manera reactiva.
    /// </summary>
    public interface IServicioRepository
    {
        /// <summary>
        /// Obtiene todos los servicios activos (no eliminados) de manera reactiva.
        /// </summary>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite una lista de <see cref="Servicio"/> activos 
        /// y completa la secuencia. 
        /// </returns>
        IObservable<IList<Servicio>> GetActiveServicesAsync();

        /// <summary>
        /// Obtiene un servicio por nombre de manera reactiva. 
        /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
        /// </summary>
        /// <param name="name">Nombre del servicio a buscar.</param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite la entidad <see cref="Servicio"/> encontrada
        /// y luego completa la secuencia.
        /// </returns>
        IObservable<Servicio> GetByNameAsync(string name);

        /// <summary>
        /// Obtiene un servicio por su identificador (ID) de manera reactiva.
        /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
        /// </summary>
        /// <param name="id">Identificador del servicio a buscar.</param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite la entidad <see cref="Servicio"/> encontrada
        /// y luego completa la secuencia.
        /// </returns>
        IObservable<Servicio> GetByIdAsync(int id);

        /// <summary>
        /// Agrega un nuevo servicio a la base de datos de manera reactiva.
        /// </summary>
        /// <param name="servicio">La entidad <see cref="Servicio"/> a persistir.</param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite la entidad <see cref="Servicio"/> 
        /// ya persistida (incluyendo el ID asignado) y completa la secuencia.
        /// </returns>
        IObservable<Servicio> AddAsync(Servicio servicio);
    }
}
