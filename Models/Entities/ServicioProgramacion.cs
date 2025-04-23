using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Models.Entities
{
    /// <summary>
    /// Representa la planificación de la ejecución para una configuración de servicio.
    /// Esta entidad almacena la información necesaria para programar cuándo se debe ejecutar
    /// un servicio, utilizando expresiones CRON para definir el horario.
    /// </summary>
    [Table("ServicioProgramacion")]
    public class ServicioProgramacion : AuditEntities
    {
        /// <summary>
        /// Identificador único de la planificación.
        /// Generado automáticamente por la base de datos.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la configuración de servicio asociada.
        /// </summary>
        [Required]
        public int ServicioConfiguracionId { get; set; }

        /// <summary>
        /// Referencia de navegación a la configuración de servicio asociada,
        /// establecida mediante la clave foránea <see cref="ServicioConfiguracionId"/>.
        /// </summary>
        [ForeignKey(nameof(ServicioConfiguracionId))]
        public ServicioConfiguracion ServicioConfiguracion { get; set; }

        /// <summary>
        /// Expresión CRON para definir la planificación.
        /// Ejemplo: "5 17 * * *" para programar la ejecución a las 17:05 todos los días.
        /// </summary>
        [Required]
        public string CronExpression { get; set; } = string.Empty;
        
        /// <summary>
        /// Fecha de comienzo
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// Fecha Fin
        /// </summary>
        public DateTime EndDate { get; set; }
        
    }
}