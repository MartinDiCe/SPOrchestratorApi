namespace SPOrchestratorAPI.Services.EndpointServices
{
    /// <summary>
    /// Define las operaciones para ejecutar llamadas a endpoints.
    /// </summary>
    public interface IEndpointService
    {
        /// <summary>
        /// Ejecuta la llamada al endpoint basado en el nombre del servicio y los parámetros especificados.
        /// </summary>
        /// <param name="serviceName">Nombre del servicio/end-point.</param>
        /// <param name="parameters">Parámetros a enviar en la llamada.</param>
        /// <returns>Un observable con la respuesta del endpoint.</returns>
        IObservable<object> EjecutarEndpointPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null);
    }
}
