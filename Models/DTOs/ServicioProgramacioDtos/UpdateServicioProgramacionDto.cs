namespace SPOrchestratorAPI.Models.DTOs.ServicioProgramacioDtos;

/// <summary>
/// DTO para actualizar una programación de servicio existente.
/// </summary>
public class UpdateServicioProgramacionDto
{
    /// <summary>
    /// Identificador único de la programación a actualizar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificador de la configuración de servicio asociada.
    /// </summary>
    public int ServicioConfiguracionId { get; set; }

    /// <summary>
    /// Nueva expresión CRON para definir la planificación.
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;
}
