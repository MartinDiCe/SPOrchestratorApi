namespace SPOrchestratorAPI.Models.DTOs
{
    /// <summary>
    /// DTO para actualizar un <see cref="Servicio"/> existente.
    /// </summary>
    public class UpdateServicioDto
    {
        /// <summary>
        /// Identificador del servicio a actualizar.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del servicio.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del servicio.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el servicio está activo (true) o inactivo (false).
        /// </summary>
        public bool Status { get; set; } = true;
    }
}