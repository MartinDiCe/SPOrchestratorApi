using System.ComponentModel.DataAnnotations;
using SPOrchestratorAPI.Helpers;

namespace SPOrchestratorAPI.Models.DTOs.ServicioDtos;

/// <summary>
/// DTO para la creación de un nuevo <see cref="Servicio"/>. 
/// Incluye los datos básicos requeridos para su registro.
/// </summary>
public class CreateServicioDto
{
    /// <summary>
    /// Nombre del servicio.
    /// </summary>
    [Required]
    [NombreFormat]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del servicio.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el servicio se crea activo (true) o inactivo (false).
    /// </summary>
    public bool Status { get; set; } = true;
}