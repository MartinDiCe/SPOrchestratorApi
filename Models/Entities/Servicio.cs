using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Models.Entities
{
    /// <summary>
    /// Representa la entidad principal de "Servicio", 
    /// la cual hereda campos de auditoría de <see cref="AuditEntities"/>.
    /// </summary>
    [Table("Servicio")]
    public class Servicio : AuditEntities
    {
        /// <summary>
        /// Identificador único del servicio.
        /// Generado automáticamente por la base de datos.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del servicio.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del servicio, con un máximo de 250 caracteres.
        /// </summary>
        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el servicio está activo (true) o inactivo (false).
        /// </summary>
        [Required]
        public bool Status { get; set; } = true;
    }
}