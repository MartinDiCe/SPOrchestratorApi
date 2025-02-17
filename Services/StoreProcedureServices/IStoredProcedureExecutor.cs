using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    /// <summary>
    /// Define los métodos para ejecutar un stored procedure según el proveedor de base de datos.
    /// Esta interfaz permite ejecutar procedimientos almacenados de forma asíncrona,
    /// ya sea para operaciones que no retornan datos (como Insert, Update, Delete) o
    /// para aquellas que sí retornan datos.
    /// </summary>
    public interface IStoredProcedureExecutor
    {
        /// <summary>
        /// Ejecuta un procedimiento almacenado de manera asíncrona sin esperar resultados (non-query).
        /// </summary>
        /// <param name="config">
        /// La configuración que contiene la información necesaria para la ejecución del stored procedure,
        /// como el nombre del SP, la cadena de conexión, el proveedor y la definición de parámetros esperados.
        /// </param>
        /// <param name="parameters">
        /// Diccionario opcional con los parámetros que se enviarán al stored procedure. Las claves deben
        /// corresponder a los nombres de los parámetros definidos en el SP.
        /// </param>
        /// <returns>
        /// Una tarea que retorna el número de filas afectadas por la ejecución del stored procedure.
        /// </returns>
        Task<int> ExecuteNonQueryAsync(ServicioConfiguracion config, IDictionary<string, object>? parameters);

        /// <summary>
        /// Ejecuta un procedimiento almacenado de manera asíncrona y retorna datos.
        /// </summary>
        /// <param name="config">
        /// La configuración que contiene la información necesaria para la ejecución del stored procedure,
        /// incluyendo el nombre del SP, la cadena de conexión, el proveedor y la configuración de parámetros.
        /// </param>
        /// <param name="parameters">
        /// Diccionario opcional con los parámetros que se enviarán al stored procedure. Las claves deben
        /// corresponder a los nombres de los parámetros definidos en el SP.
        /// </param>
        /// <returns>
        /// Una tarea que retorna un objeto que contiene los datos obtenidos, típicamente una lista de diccionarios,
        /// donde cada diccionario representa una fila del resultado.
        /// </returns>
        Task<object> ExecuteReaderAsync(ServicioConfiguracion config, IDictionary<string, object>? parameters);
    }
}