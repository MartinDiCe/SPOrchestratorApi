using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;

/// <summary>
/// Define las operaciones de acceso a datos para la entidad <see cref="ServicioConfiguracion"/> 
/// de manera reactiva.
/// </summary>
public interface IServicioConfiguracionRepository
{
    /// <summary>
    /// Obtiene la configuración de un servicio por su <paramref name="id"/>, 
    /// excluyendo las configuraciones marcadas como eliminadas (si manejas soft-delete).
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
    /// </summary>
    IObservable<Entities.ServicioConfiguracion> GetByIdAsync(int id);

    /// <summary>
    /// Crea una nueva configuración de servicio en la base de datos.
    /// </summary>
    IObservable<Entities.ServicioConfiguracion> CreateAsync(Entities.ServicioConfiguracion config);

    /// <summary>
    /// Actualiza una configuración de servicio existente.
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
    /// </summary>
    IObservable<Entities.ServicioConfiguracion> UpdateAsync(Entities.ServicioConfiguracion config);

    /// <summary>
    /// Elimina lógicamente una configuración (soft-delete).
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
    /// </summary>
    IObservable<Entities.ServicioConfiguracion> SoftDeleteAsync(int id);

    /// <summary>
    /// Restaura una configuración previamente eliminada lógicamente.
    /// Lanza <see cref="ResourceNotFoundException"/> si no se encuentra.
    /// </summary>
    IObservable<Entities.ServicioConfiguracion> RestoreAsync(int id);

    /// <summary>
    /// Obtiene todas las configuraciones (no eliminadas) 
    /// y opcionalmente las asocia con su <see cref="Servicio"/> si lo requieres.
    /// </summary>
    IObservable<IList<Entities.ServicioConfiguracion>> GetAllAsync();

    /// <summary>
    /// Obtiene la configuración asociada a un <see cref="Servicio"/> específico, 
    /// excluyendo configuraciones eliminadas.
    /// Podrías filtrar por <paramref name="servicioId"/> o <paramref name="servicioName"/>.
    /// </summary>
    IObservable<IList<Entities.ServicioConfiguracion>> GetByServicioIdAsync(int servicioId);
}