namespace SPOrchestratorAPI.Models.DTOs.ParameterDtos
{
    /// <summary>
    /// DTO para la creación de un nuevo parámetro global.
    /// </summary>
    public class CreateParameterDto
    {
        /// <summary>
        /// Nombre del parámetro (por ejemplo, "GlobalTimeoutSeconds").
        /// </summary>
        public string ParameterName { get; set; } = string.Empty;

        /// <summary>
        /// Valor del parámetro, en forma de cadena.
        /// </summary>
        public string ParameterValue { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional del parámetro.
        /// </summary>
        public string? ParameterDescription { get; set; }
        
        /// <summary>
        /// Categoría del parámetro.
        /// </summary>
        public string? ParameterCategory { get; set; }
    }
}