using System.ComponentModel.DataAnnotations;

namespace SPOrchestratorAPI.Models.Base
{
    /// <summary>
    /// Clase base que encapsula los campos de auditoría (fechas, usuario) 
    /// para entidades que requieren seguimiento de creación, modificación y eliminación.
    /// </summary>
    public abstract class AuditEntities
    {
        /// <summary>
        /// Fecha/hora de creación de la entidad (en UTC).
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario o proceso que creó la entidad.
        /// </summary>
        [Required]
        public string CreatedBy { get; set; } = "System";

        /// <summary>
        /// Fecha/hora de la última actualización de la entidad (en UTC). 
        /// Nulo si nunca ha sido modificada.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuario o proceso que realizó la última actualización de la entidad.
        /// Nulo si nunca ha sido modificada.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Fecha/hora en la que se eliminó lógicamente la entidad (en UTC).
        /// Nulo si la entidad no ha sido eliminada.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Usuario o proceso que marcó la entidad como eliminada.
        /// Nulo si la entidad no ha sido eliminada.
        /// </summary>
        public string? DeletedBy { get; set; }

        /// <summary>
        /// Indica si la entidad está marcada como eliminada lógicamente.
        /// </summary>
        [Required]
        public bool Deleted { get; set; } = false;
    }
}