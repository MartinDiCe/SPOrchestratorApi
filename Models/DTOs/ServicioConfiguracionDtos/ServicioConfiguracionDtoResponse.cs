namespace SPOrchestratorAPI.Models.DTOs.ServicioConfiguracionDtos;

/// <summary>
/// DTO que representa la respuesta al consultar una <see cref="ServicioConfiguracion"/>,
/// incluyendo campos de auditoría.
/// </summary>
public class ServicioConfiguracionDtoResponse
{
    /// <summary>
    /// Identificador de la configuración.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Identificador del <see cref="Servicio"/> al que pertenece esta configuración.
    /// </summary>
    public int ServicioId { get; init; }

    /// <summary>
    /// Nombre del procedimiento almacenado.
    /// </summary>
    public string NombreProcedimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cadena de conexión a la base de datos del SP.
    /// </summary>
    public string ConexionBaseDatos { get; set; } = string.Empty;

    /// <summary>
    /// Parámetros del SP, en formato JSON.
    /// </summary>
    public string Parametros { get; set; } = string.Empty;

    /// <summary>
    /// Número máximo de reintentos.
    /// </summary>
    public int MaxReintentos { get; set; }

    /// <summary>
    /// Tiempo máximo de espera (en segundos) para la ejecución.
    /// </summary>
    public int TimeoutSegundos { get; set; }

    /// <summary>
    /// Fecha/hora de creación de la configuración.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Usuario o proceso que creó la configuración.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Fecha/hora de la última actualización de la configuración.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuario o proceso que realizó la última actualización.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Indica si se ha marcado la configuración como eliminada (soft delete).
    /// </summary>
    public bool? Deleted { get; set; }

    /// <summary>
    /// Usuario que marcó la configuración como eliminada.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Fecha/hora en que se marcó la configuración como eliminada.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}