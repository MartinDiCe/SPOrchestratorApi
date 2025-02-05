namespace SPOrchestratorAPI.Models.DTOs.ServicioConfiguracionDtos;

/// <summary>
/// DTO para la creación de una nueva configuración de <see cref="ServicioConfiguracion"/>,
/// asociada a un <see cref="Servicio"/> existente.
/// </summary>
public class CreateServicioConfiguracionDto
{
    /// <summary>
    /// Identificador del <see cref="Servicio"/> al que pertenece esta configuración.
    /// </summary>
    public int ServicioId { get; set; }

    /// <summary>
    /// Nombre del procedimiento almacenado (Stored Procedure) que se ejecutará.
    /// </summary>
    public string NombreProcedimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cadena de conexión a la base de datos donde se ubica el procedimiento.
    /// </summary>
    public string ConexionBaseDatos { get; set; } = string.Empty;

    /// <summary>
    /// Parámetros del SP, usualmente en formato JSON para mayor flexibilidad.
    /// </summary>
    public string Parametros { get; set; } = string.Empty;

    /// <summary>
    /// Máximo número de reintentos permitidos antes de declarar un fallo en la ejecución.
    /// </summary>
    public int MaxReintentos { get; set; }

    /// <summary>
    /// Tiempo máximo de espera (en segundos) antes de cancelar la ejecución del SP.
    /// </summary>
    public int TimeoutSegundos { get; set; }
}