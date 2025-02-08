using System.ComponentModel.DataAnnotations;

namespace SPOrchestratorAPI.Models.Entities
{
    /// <summary>
    /// Representa el registro de traza de una solicitud y respuesta de la API.
    /// </summary>
    public class ApiTrace
    {
        /// <summary>
        /// Identificador único del registro.
        /// </summary>
        [Key]
        public int ApiTraceId { get; set; }

        /// <summary>
        /// (Opcional) Identificador del servicio al que se llamó.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Nombre o endpoint del servicio invocado.
        /// </summary>
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// IP o información del origen de la solicitud.
        /// </summary>
        [MaxLength(50)]
        public string RequestOrigin { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora en que se realizó la solicitud.
        /// </summary>
        public DateTime RequestTimestamp { get; set; }

        /// <summary>
        /// Método HTTP de la solicitud (GET, POST, PUT, etc.).
        /// </summary>
        [MaxLength(10)]
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// Cuerpo de la solicitud.
        /// </summary>
        public string RequestPayload { get; set; } = string.Empty;

        /// <summary>
        /// Cuerpo de la respuesta.
        /// </summary>
        public string ResponsePayload { get; set; } = string.Empty;

        /// <summary>
        /// Tiempo de ejecución de la solicitud en segundos.
        /// </summary>
        public double ExecutionTimeSeconds { get; set; }

        /// <summary>
        /// Peso total de la transmisión (por ejemplo, en bytes).
        /// </summary>
        public int PayloadSize { get; set; }

        /// <summary>
        /// Código HTTP de la respuesta.
        /// </summary>
        public int HttpResponseCode { get; set; }
    }
}