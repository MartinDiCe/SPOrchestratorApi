namespace SPOrchestratorAPI.Models.DTOs.StoreProcedureDtos
{
    /// <summary>
    /// DTO para solicitar la ejecución final de un stored procedure.
    /// El cliente debe enviar el nombre del servicio y los valores de los parámetros.
    /// La configuración completa (nombre del SP, cadena de conexión, proveedor y parámetros esperados)
    /// se obtiene de la base de datos a partir del nombre del servicio.
    /// </summary>
    public class StoredProcedureExecutionRequest
    {
        /// <summary>
        /// Nombre del servicio (Servicio.Name) que identifica la configuración que se usará.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Diccionario con los valores de los parámetros a enviar al stored procedure.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
        
        /// <summary>
        /// Indica si la respuesta debe devolverse como archivo (true) o como JSON (false).
        /// </summary>
        public bool IsFile { get; set; }
        
    }
}