using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Models.Entities
{
    /// <summary>
    /// Representa la configuración necesaria para que un 
    /// <see cref="Servicio"/> ejecute un Stored Procedure, VistaSQL o EndPoint.
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
        /// Nombre del procedimiento almacenado, vista SQL o endpoint que el servicio ejecutará.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string NombreProcedimiento { get; set; } = string.Empty;

        /// <summary>
        /// Cadena de conexión a la base de datos donde se ubica el procedimiento o vista.
        /// Para configuraciones de tipo EndPoint, se asigna "No requiere".
        /// </summary>
        [Required]
        public string ConexionBaseDatos { get; set; } = string.Empty;

        /// <summary>
        /// Parámetros opcionales (en formato JSON) requeridos para el proceso.
        /// Puede ser nulo si el proceso no requiere parámetros o se gestionan externamente.
        /// </summary>
        public string? Parametros { get; set; }

        /// <summary>
        /// Número máximo de reintentos permitido antes de marcar la ejecución como fallida.
        /// </summary>
        public int MaxReintentos { get; set; } = 3;

        /// <summary>
        /// Tiempo máximo de espera (en segundos) antes de cancelar la ejecución del proceso.
        /// </summary>
        public int TimeoutSegundos { get; set; } = 30;
        
        /// <summary>
        /// Proveedor de la base de datos o endpoint.
        /// </summary>
        [Required]
        public DatabaseProvider Provider { get; set; }
        
        /// <summary>
        /// Tipo de configuración para el servicio, que define la acción a ejecutar 
        /// (por ejemplo, StoredProcedure, VistaSql, EndPoint, etc.).
        /// </summary>
        [Required]
        public TipoConfiguracion Tipo { get; set; }
        
        /// <summary>
        /// Indica si la ejecución de la configuración se realizará de forma programada.
        /// Si es <c>true</c>, se ejecutará según una programación definida; en caso contrario, se ejecutará de forma inmediata o manual.
        /// </summary>
        [Required]
        public bool EsProgramado { get; set; }
        
        /// <summary>
        /// Indica si se deben guardar los registros de la ejecución para auditoría o reprocesamiento.
        /// </summary>
        [Required]
        public bool GuardarRegistros { get; set; } = false;
        
        /// <summary>
        /// Indica si, al finalizar la ejecución del proceso, se debe invocar un proceso de continuación.
        /// </summary>
        [Required]
        public bool ContinuarCon { get; set; } = false;
        
        /// <summary>
        /// Configuracion usada solo si el provider es un endpoint.
        /// </summary>
        public string JsonConfig { get; set; } = string.Empty;
    }
}
