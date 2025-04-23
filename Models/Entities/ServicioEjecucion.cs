using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Models.Entities
{
    /// <summary>
    /// Registra la ejecución de un proceso (Stored Procedure, Vista SQL o EndPoint) para auditoría y reprocesamiento.
    /// </summary>
    [Table("ServicioEjecucion")]
    public class ServicioEjecucion : AuditEntities
    {
        /// <summary>
        /// Identificador único de la ejecución.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Identificador del servicio que se ejecutó.
        /// </summary>
        [Required]
        public int ServicioId { get; set; }

        /// <summary>
        /// Identificador de la configuración utilizada para la ejecución.
        /// </summary>
        [Required]
        public int ServicioConfiguracionId { get; set; }
        
        /// <summary>
        /// Identificador del servicio padre que hizo ejecutar.
        /// </summary>
        public int? ServicioDesencadenadorId { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó la ejecución.
        /// </summary>
        [Required]
        public DateTime FechaEjecucion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Duración de la ejecución en segundos.
        /// </summary>
        public double Duracion { get; set; }

        /// <summary>
        /// Indica el estado de la ejecución: true si fue exitosa, false si falló.
        /// </summary>
        [Required]
        public bool Estado { get; set; }

        /// <summary>
        /// Mensaje de error en caso de que la ejecución falle.
        /// </summary>
        public string? MensajeError { get; set; }

        /// <summary>
        /// Parámetros de entrada utilizados en la ejecución (en formato JSON).
        /// </summary>
        public string? Parametros { get; set; }

        /// <summary>
        /// Resultado de la ejecución (en formato JSON). Este campo es opcional.
        /// </summary>
        public string? Resultado { get; set; }

        /// <summary>
        /// Campo genérico para almacenar información adicional de la ejecución (en formato JSON).
        /// </summary>
        public string? CamposExtra { get; set; }
        
        /// <summary>Referencia al registro de ejecución que desencadenó éste.</summary>
        public int? ServicioEjecucionDesencadenadorId { get; set; }
    
        /// <summary>Navegación al desencadenador.</summary>
        public ServicioEjecucion? ServicioEjecucionDesencadenador { get; set; }
    
        /// <summary>Ejecutiones que este registro desencadenó.</summary>
        public ICollection<ServicioEjecucion> Hijos { get; set; } = new List<ServicioEjecucion>();
        
    }
}
