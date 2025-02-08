namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    public interface IStoredProcedureService
    {
        /// <summary>
        /// Ejecuta un procedimiento almacenado de forma asíncrona y reactiva.
        /// </summary>
        /// <param name="spName">Nombre del procedimiento almacenado.</param>
        /// <param name="parameters">
        /// Diccionario opcional con los parámetros a enviar al procedimiento.  
        /// Si es nulo o vacío, se ejecutará sin parámetros.
        /// </param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite un <see cref="int"/> con el número de filas afectadas.
        /// </returns>
        IObservable<int> EjecutarSpAsync(int idConfiguracion, IDictionary<string, object>? parameters = null);

        /// <summary>
        /// Ejecuta un stored procedure utilizando la configuración asociada al servicio identificado por <paramref name="serviceName"/>.
        /// Se espera que el request incluya el nombre del servicio y un diccionario con los valores de los parámetros.
        /// La configuración (nombre del SP, cadena de conexión, proveedor y parámetros esperados) se obtiene de la base de datos.
        /// Se valida que se envíen exactamente los parámetros esperados.
        /// El resultado del SP se transforma en una lista de diccionarios para ser serializado a JSON.
        /// </summary>
        /// <param name="serviceName">Nombre del servicio (Servicio.Name) a partir del cual se obtiene la configuración.</param>
        /// <param name="parameters">
        /// Diccionario con los valores de los parámetros a enviar. Si es null o está vacío, se ejecuta sin parámetros.
        /// </param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite un objeto (por ejemplo, un List de Dictionary) representando el conjunto de resultados del SP.
        /// </returns>
        IObservable<object> EjecutarSpConRespuestaPorNombreAsync(String ServiceName,
            IDictionary<string, object>? parameters = null);
    }
}