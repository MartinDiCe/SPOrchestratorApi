using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Models.Entities;

/// <summary>
/// Representa la configuración necesaria para que un 
/// <see cref="Servicio"/> ejecute un Stored Procedure específico.
/// </summary>
[Table("ServicioConfiguracion")]
public class ServicioConfiguracion : AuditEntities
{
    /// <summary>
    /// Identificador único de la configuración.
    /// Generado automáticamente por la base de datos.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Identificador del <see cref="Servicio"/> asociado.
    /// </summary>
    [Required]
    public int ServicioId { get; set; }

    /// <summary>
    /// Referencia de navegación al <see cref="Servicio"/> asociado,
    /// establecida mediante la clave foránea <see cref="ServicioId"/>.
    /// </summary>
    [ForeignKey(nameof(ServicioId))]
    public required Servicio Servicio { get; set; }

    /// <summary>
    /// Nombre del procedimiento almacenado (Stored Procedure), VistaSQL o ENDPOINT que el servicio ejecutará.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string NombreProcedimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cadena de conexión a la base de datos donde se ubica el procedimiento almacenado.
    /// </summary>
    [Required]
    public string ConexionBaseDatos { get; set; } = string.Empty;

    /// <summary>
    /// Parámetros opcionales (en formato JSON) requeridos para el Stored Procedure.
    /// Puede ser nulo si el SP no requiere parámetros o se gestionan externamente.
    /// </summary>
    public string? Parametros { get; set; }

    /// <summary>
    /// Número máximo de reintentos permitido antes de marcar la ejecución como fallida.
    /// </summary>
    public int MaxReintentos { get; set; } = 3;

    /// <summary>
    /// Tiempo máximo de espera (en segundos) antes de cancelar la ejecución del procedimiento.
    /// </summary>
    public int TimeoutSegundos { get; set; } = 30;
    
    /// <summary>
    /// Base de datos de la conexion.
    /// </summary>
    [Required]
    public DatabaseProvider Provider { get; set; } 
    
    /// <summary>
    /// Tipo de configuración para el servicio, que define la acción a ejecutar (por ejemplo, StoredProcedure, VistaSql, EndPoint, etc.).
    /// </summary>
    [Required]
    public TipoConfiguracion Tipo { get; set; } 
    
    /// <summary>
    /// Indica si la ejecución de la configuración se realizará de forma programada.
    /// Si es <c>true</c>, se ejecutará según una programación definida; en caso contrario, se ejecutará de forma inmediata o manual.
    /// </summary>
    [Required]
    public bool EsProgramado { get; set; } 

}