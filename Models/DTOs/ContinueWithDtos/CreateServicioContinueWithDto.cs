using System.ComponentModel.DataAnnotations;

namespace SPOrchestratorAPI.Models.DTOs.ContinueWithDtos
{
    /// <summary>
    /// DTO para crear un nuevo mapeo de continuación.
    /// </summary>
    public class CreateServicioContinueWithDto
    {
        /// <summary>
        /// Identificador de la configuración del servicio inicial.
        /// </summary>
        [Required(ErrorMessage = "El ID de configuración es obligatorio.")]
        public int ServicioConfiguracionId { get; set; }

        /// <summary>
        /// Identificador del servicio de continuación (el proceso al que se enviarán los datos).
        /// </summary>
        [Required(ErrorMessage = "El ID del servicio de continuación es obligatorio.")]
        public int ServicioContinuacionId { get; set; }

        /// <summary>
        /// Cadena que define el mapeo de campos entre el resultado del proceso inicial y los parámetros del servicio de continuación.
        /// Formato sugerido: "CampoResultado=ParametroContinuacion;OtroCampo=OtroParametro"
        /// </summary>
        [Required(ErrorMessage = "La cadena de mapeo (CamposRelacion) es obligatoria.")]
        public string CamposRelacion { get; set; } = string.Empty;
        
    }
}
