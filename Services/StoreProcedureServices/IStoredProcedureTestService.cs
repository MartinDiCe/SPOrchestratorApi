namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    /// <summary>
    /// Define las operaciones para ejecutar stored procedures que no retornan datos (operación NonQuery).
    /// </summary>
    public interface IStoredProcedureTestService
    {
        /// <summary>
        /// Ejecuta un stored procedure basado en la configuración identificada por <paramref name="idConfiguracion"/>,
        /// validando los parámetros y retornando el número de filas afectadas.
        /// </summary>
        /// <param name="idConfiguracion">
        /// El identificador de la configuración del stored procedure que contiene detalles como el nombre del SP,
        /// cadena de conexión y parámetros esperados.
        /// </param>
        /// <param name="parameters">
        /// Diccionario opcional de parámetros a enviar al stored procedure. Las claves deben coincidir con los nombres
        /// de los parámetros definidos en la configuración.
        /// </param>
        /// <returns>
        /// Un observable que retorna el número de filas afectadas por la ejecución del stored procedure.
        /// </returns>
        IObservable<int> EjecutarSpAsync(int idConfiguracion, IDictionary<string, object>? parameters = null);
    }
}