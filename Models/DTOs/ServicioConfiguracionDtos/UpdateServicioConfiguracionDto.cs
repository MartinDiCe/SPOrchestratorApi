using System.ComponentModel.DataAnnotations;
using SPOrchestratorAPI.Models.Enums;
using SPOrchestratorAPI.Validations;

namespace SPOrchestratorAPI.Models.DTOs.ServicioConfiguracionDtos;

/// <summary>
/// DTO para actualizar una <see cref="ServicioConfiguracion"/> existente.
/// </summary>
public class UpdateServicioConfiguracionDto
{
    /// <summary>
    /// Identificador de la configuración a actualizar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificador del <see cref="Servicio"/> al que pertenece esta configuración.
    /// </summary>
    public int ServicioId { get; set; }

    /// <summary>
    /// Nombre del procedimiento almacenado.
    /// </summary>
    public string NombreProcedimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cadena de conexión a la base de datos del procedimiento.
    /// </summary>
    [ConnectionStringValidation]
    public string ConexionBaseDatos { get; set; } = string.Empty;

    /// <summary>
    /// Parámetros del SP, en formato JSON.
    /// </summary>
    public string Parametros { get; set; } = string.Empty;

    /// <summary>
    /// Número máximo de reintentos antes de fallar la ejecución.
    /// </summary>
    public int MaxReintentos { get; set; }

    /// <summary>
    /// Tiempo máximo de espera (en segundos) antes de cancelar la ejecución del SP.
    /// </summary>
    public int TimeoutSegundos { get; set; }
    
    /// <summary>
    /// Base de datos de la conexion.
    /// </summary>
    [Required]
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;
}