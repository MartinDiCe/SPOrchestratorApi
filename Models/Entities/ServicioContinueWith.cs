using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Models.Entities
{
    /// <summary>
    /// Representa el mapeo de campos para la continuación de un proceso.
    /// Permite definir qué campo del resultado del proceso inicial se asigna a qué parámetro
    /// en el proceso de continuación, estableciendo la relación con la configuración inicial
    /// y el servicio de continuación.
    /// </summary>
    [Table("ServicioContinueWith")]
    public class ServicioContinueWith : AuditEntities
    {
        /// <summary>
        /// Identificador único del mapeo.
        /// Generado automáticamente por la base de datos.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// Identificador de la configuración del servicio inicial al que se asocia este mapeo.
        /// </summary>
        [Required]
        public int ServicioConfiguracionId { get; set; }
        
        /// <summary>
        /// Identificador del servicio de continuación (el proceso al que se enviarán los datos).
        /// </summary>
        [Required]
        public int ServicioContinuacionId { get; set; }
        
        /// <summary>
        /// Cadena que define el mapeo de campos entre el resultado del proceso inicial y
        /// los parámetros del servicio de continuación.
        /// Formato sugerido: "CampoResultado=ParametroContinuacion;OtroCampo=OtroParametro"
        /// </summary>
        [Required]
        public string CamposRelacion { get; set; } = string.Empty;
    }
}