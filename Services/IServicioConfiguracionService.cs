using System.Reactive;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services;

/// <summary>
/// Interfaz para el servicio de configuración de servicios.
/// </summary>
public interface IServicioConfiguracionService
{
    /// <summary>
    /// Obtiene todas las configuraciones de servicios activas de manera reactiva.
    /// </summary>
    IObservable<IEnumerable<ServicioConfiguracion>> GetAllAsync();

    /// <summary>
    /// Obtiene la configuración de un servicio por su ID de manera reactiva.
    /// </summary>
    IObservable<ServicioConfiguracion> GetByServicioIdAsync(int servicioId);

    /// <summary>
    /// Crea una nueva configuración de servicio de manera reactiva.
    /// </summary>
    IObservable<ServicioConfiguracion> CreateAsync(ServicioConfiguracion config);

    /// <summary>
    /// Actualiza una configuración existente de manera reactiva.
    /// </summary>
    IObservable<Unit> UpdateAsync(ServicioConfiguracion config);

    /// <summary>
    /// Marca una configuración como eliminada (eliminación lógica) de manera reactiva.
    /// </summary>
    IObservable<Unit> DeleteBySystemAsync(int id);

    /// <summary>
    /// Restaura una configuración eliminada de manera reactiva.
    /// </summary>
    IObservable<Unit> RestoreBySystemAsync(int id);
}