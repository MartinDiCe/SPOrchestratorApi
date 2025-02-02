using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Models.Entities;

/// <summary>
/// Representa la configuración de un servicio que ejecuta un Stored Procedure.
/// </summary>
[Table("ServicioConfiguracion")]
public class ServicioConfiguracion : AuditEntities
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int ServicioId { get; set; }

    [ForeignKey("ServicioId")]
    public required Servicio Servicio { get; set; }

    [Required]
    [MaxLength(200)]
    public string NombreProcedimiento { get; set; } = string.Empty;

    [Required]
    public string ConexionBaseDatos { get; set; } = string.Empty;

    public string? Parametros { get; set; } // JSON con los parámetros requeridos

    public int MaxReintentos { get; set; } = 3;

    public int TimeoutSegundos { get; set; } = 30;
}