// IChainOrchestratorService.cs
namespace SPOrchestratorAPI.Services.ChainOrchestratorServices
{
    /// <summary>
    /// Interfaz para la orquestación en cadena, que permite ejecutar un servicio
    /// y encadenar su continuación según la configuración (Continue-With).
    /// </summary>
    public interface IChainOrchestratorService
    {
        /// <summary>
        /// Ejecuta un servicio principal y, si está configurado, encadena su continuación.
        /// </summary>
        /// <param name="serviceName">Nombre del servicio a ejecutar.</param>
        /// <param name="parameters">Parámetros iniciales para el servicio.</param>
        /// <returns>Flujo reactivo con el resultado del servicio principal y sus continuaciones.</returns>
        IObservable<object> EjecutarConContinuacionAsync(
            string serviceName,
            IDictionary<string, object>? parameters = null);
    }
}