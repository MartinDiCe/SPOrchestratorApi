namespace SPOrchestratorAPI.Services.SPOrchestratorServices
{
    /// <summary>
    /// Define la interfaz para el servicio orquestador de SP, vistas SQL y endpoints
    /// que ejecuta un proceso por nombre y registra auditoría según configuración.
    /// </summary>
    public interface ISpOrchestratorService
    {
        /// <summary>
        /// Ejecuta el proceso configurado identificado por <paramref name="serviceName"/>.
        /// </summary>
        /// <param name="serviceName">
        /// Nombre único del servicio (SP, vista o endpoint) configurado en base de datos.
        /// </param>
        /// <param name="parameters">
        /// Diccionario de parámetros a pasar a la ejecución. Puede ser <c>null</c>.
        /// </param>
        /// <param name="skipAudit">
        /// Indica si se debe omitir el registro de auditoría aun cuando la configuración lo solicite.
        /// </param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> conteniendo el resultado de la ejecución. El flujo
        /// se completa cuando termina la operación.
        /// </returns>
        IObservable<object> EjecutarPorNombreAsync(
            string serviceName,
            IDictionary<string, object>? parameters = null,
            bool skipAudit = false
        );
    }
}