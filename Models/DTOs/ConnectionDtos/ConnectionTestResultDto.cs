namespace SPOrchestratorAPI.Models.DTOs.ConnectionDtos
{
    public class ConnectionTestResultDto
    {
        /// <summary>
        /// Indica si la conexión fue exitosa.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje de error, en caso de fallo.
        /// </summary>
        public string? ExceptionMessage { get; set; }
    }
}