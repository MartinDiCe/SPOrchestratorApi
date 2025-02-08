namespace SPOrchestratorAPI.Models.DTOs.StoreProcedureDtos;

/// <summary>
/// DTO para la solicitud de ejecución de un stored procedure.
/// </summary>
public class StoredProcedureTestRequest
{
    /// <summary>
    /// Identificador de la configuración (ServicioConfiguracion) que se utilizará para ejecutar el SP.
    /// </summary>
    public int IdConfiguracion { get; set; }

    /// <summary>
    /// Diccionario opcional con parámetros para el SP.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}
