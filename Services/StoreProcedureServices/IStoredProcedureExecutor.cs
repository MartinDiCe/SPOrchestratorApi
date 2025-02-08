namespace SPOrchestratorAPI.Services.StoreProcedureServices;

/// <summary>
/// Define la interfaz para la ejecución directa de un Stored Procedure basado en el nombre del servicio.
/// Esta interfaz se encarga de obtener la configuración del SP, validar los parámetros y ejecutar el SP utilizando
/// la configuración almacenada en la base de datos.
/// </summary>
public interface IStoredProcedureExecutor
{
    
    /// <summary>
    /// Ejecuta el stored procedure asociado al servicio especificado utilizando los parámetros proporcionados.
    /// La configuración completa (nombre del SP, cadena de conexión, proveedor y parámetros esperados) se obtiene
    /// a partir del nombre del servicio.
    /// </summary>
    /// <param name="serviceName">
    /// El nombre del servicio (corresponde a <see cref="Servicio.Name"/>) para el cual se debe ejecutar el stored procedure.
    /// </param>
    /// <param name="parameters">
    /// Un diccionario opcional que contiene los valores de los parámetros a enviar al stored procedure.
    /// Si es <c>null</c> o está vacío, el stored procedure se ejecutará sin parámetros.
    /// </param>
    /// <param name="cancellationToken">
    /// Un token de cancelación que permite abortar la operación en caso de que se exceda el timeout individual de la ejecución.
    /// </param>
    /// <returns>
    /// Una tarea que, al completarse, devuelve un objeto que representa el resultado de la ejecución del stored procedure.
    /// Por ejemplo, puede ser una lista de diccionarios, donde cada diccionario representa una fila del resultado.
    /// </returns>
    /// <remarks>
    /// Esta interfaz se utiliza en conjunto con políticas de reintentos y timeout (por ejemplo, mediante la implementación
    /// de <see cref="IRetryPolicy"/>) para gestionar de forma reactiva la ejecución de stored procedures.
    /// </remarks>
    Task<object> ExecuteAsync(string serviceName, IDictionary<string, object>? parameters, CancellationToken cancellationToken);
    
}