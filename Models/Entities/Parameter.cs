using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Models.Entities
{
    /// <summary>
    /// Representa un parámetro global del sistema.
    /// Estos parámetros se utilizan para configurar valores globales como el timeout total, umbrales del circuit breaker, etc.
    /// </summary>
    [Table("Parameter")]
    public class Parameter : AuditEntities
    {
        [Key]
        public int ParameterId { get; set; }

        /// <summary>
        /// Nombre del parámetro (por ejemplo, "GlobalTimeoutSeconds").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ParameterName { get; set; } = string.Empty;

        /// <summary>
        /// Valor del parámetro, en forma de cadena.
        /// Se puede parsear al tipo adecuado según el parámetro.
        /// </summary>
        [Required]
        public string ParameterValue { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del parámetro.
        /// </summary>
        [MaxLength(500)]
        public string? ParameterDescription { get; set; }
        
        /// <summary>
        /// Categoría del parámetro.
        /// </summary>
        [MaxLength(500)]
        public string? ParameterCategory { get; set; }
        
    }
}