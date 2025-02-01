using System.Reactive;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services;

/// <summary>
/// Interfaz para la gestión de servicios.
/// </summary>
public interface IServicioService
{
    /// <summary>
    /// Obtiene todos los servicios activos (excluyendo eliminados) de manera reactiva.
    /// </summary>
    IObservable<IEnumerable<Servicio>> GetAllAsync();

    /// <summary>
    /// Obtiene un servicio por su ID de manera reactiva.
    /// </summary>
    IObservable<Servicio> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un servicio por su nombre de manera reactiva.
    /// </summary>
    IObservable<Servicio> GetByNameAsync(string name);

    /// <summary>
    /// Crea un nuevo servicio de manera reactiva.
    /// </summary>
    IObservable<Servicio> CreateAsync(Servicio servicio);

    /// <summary>
    /// Actualiza un servicio existente de manera reactiva.
    /// </summary>
    IObservable<Unit> UpdateAsync(Servicio servicio);

    /// <summary>
    /// Cambia el estado de un servicio de manera reactiva.
    /// </summary>
    IObservable<Unit> ChangeStatusAsync(int id, bool newStatus);

    /// <summary>
    /// Marca un servicio como eliminado (eliminación lógica) de manera reactiva.
    /// </summary>
    IObservable<Unit> DeleteBySystemAsync(int id);

    /// <summary>
    /// Restaura un servicio eliminado de manera reactiva.
    /// </summary>
    IObservable<Unit> RestoreBySystemAsync(int id);
}