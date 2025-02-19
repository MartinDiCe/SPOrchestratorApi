using System.ComponentModel.DataAnnotations;

namespace SPOrchestratorAPI.Models.DTOs.ContinueWithDtos
{
    /// <summary>
    /// DTO para actualizar un mapeo de continuación existente.
    /// </summary>
    public class UpdateServicioContinueWithDto
    {
        /// <summary>
        /// Identificador único del mapeo a actualizar.
        /// </summary>
        [Required(ErrorMessage = "El ID es obligatorio.")]
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la configuración del servicio inicial.
        /// </summary>
        [Required(ErrorMessage = "El ID de configuración es obligatorio.")]
        public int ServicioConfiguracionId { get; set; }

        /// <summary>
        /// Identificador del servicio de continuación.
        /// </summary>
        [Required(ErrorMessage = "El ID del servicio de continuación es obligatorio.")]
        public int ServicioContinuacionId { get; set; }

        /// <summary>
        /// Cadena de mapeo de campos para la continuación.
        /// </summary>
        [Required(ErrorMessage = "La cadena de mapeo (CamposRelacion) es obligatoria.")]
        public string CamposRelacion { get; set; } = string.Empty;
    }
}
