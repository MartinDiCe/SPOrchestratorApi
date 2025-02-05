using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ServicioRepositories;

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
    IObservable<IList<Entities.Servicio>> GetActiveServicesAsync();

    /// <summary>
    /// Obtiene todos los servicios, independientemente de si están inactivos.
    /// </summary>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite una lista de <see cref="Servicio"/> (sin filtrar)
    /// y completa la secuencia.
    /// </returns>
    IObservable<IList<Entities.Servicio>> GetAllAsync();

    /// <summary>
    /// Obtiene un servicio por nombre de manera reactiva. 
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
    /// </summary>
    /// <param name="name">Nombre del servicio a buscar.</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite la entidad <see cref="Servicio"/> encontrada
    /// y luego completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> GetByNameAsync(string name);

    /// <summary>
    /// Obtiene un servicio por su identificador (ID) de manera reactiva.
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
    /// </summary>
    /// <param name="id">Identificador del servicio a buscar.</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite la entidad <see cref="Servicio"/> encontrada
    /// y luego completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> GetByIdAsync(int id);

    /// <summary>
    /// Agrega un nuevo servicio a la base de datos de manera reactiva.
    /// </summary>
    /// <param name="servicio">La entidad <see cref="Servicio"/> a persistir.</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite la entidad <see cref="Servicio"/> 
    /// ya persistida (incluyendo el ID asignado) y completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> AddAsync(Entities.Servicio servicio);

    /// <summary>
    /// Marca un servicio como eliminado de forma lógica (soft delete).
    /// Establece la propiedad <c>Deleted = true</c> y asigna <c>DeletedAt</c>, <c>DeletedBy</c>.
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra el servicio.
    /// </summary>
    /// <param name="id">Identificador del servicio a eliminar lógicamente.</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite el servicio tras marcarlo como eliminado
    /// y completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> SoftDeleteAsync(int id);

    /// <summary>
    /// Restaura un servicio que fue eliminado lógicamente (soft delete).
    /// Establece la propiedad <c>Deleted = false</c> y limpia <c>DeletedAt</c>, <c>DeletedBy</c>.
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra el servicio.
    /// </summary>
    /// <param name="id">Identificador del servicio a restaurar.</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite el servicio restaurado y completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> RestoreAsync(int id);

    /// <summary>
    /// Inactiva un servicio, estableciendo <c>Status = false</c>.
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra el servicio.
    /// </summary>
    /// <param name="id">Identificador del servicio a inactivar.</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite el servicio con <c>Status = false</c> y completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> DeactivateAsync(int id);

    /// <summary>
    /// Activa un servicio, estableciendo <c>Status = true</c>.
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra el servicio.
    /// </summary>
    /// <param name="id">Identificador del servicio a activar.</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite el servicio con <c>Status = true</c> y completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> ActivateAsync(int id);
    
    /// <summary>
    /// Actualiza un servicio existente con los datos proporcionados. 
    /// Lanza <see cref="ResourceNotFoundException"/> si el servicio no se encuentra (o si está eliminado).
    /// </summary>
    /// <param name="servicio">La entidad <see cref="Servicio"/> con los datos actualizados (debe incluir el Id).</param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que emite la entidad <see cref="Servicio"/> ya persistida 
    /// (incluyendo los cambios) y completa la secuencia.
    /// </returns>
    IObservable<Entities.Servicio> UpdateAsync(Entities.Servicio servicio);
}