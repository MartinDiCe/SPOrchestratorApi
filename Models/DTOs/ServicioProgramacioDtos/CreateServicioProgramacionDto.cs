namespace SPOrchestratorAPI.Models.DTOs.ServicioProgramacioDtos;

/// <summary>
/// DTO para crear una nueva programación de servicio.
/// </summary>
public class CreateServicioProgramacionDto
{
    /// <summary>
    /// Identificador de la configuración de servicio asociada.
    /// </summary>
    public int ServicioConfiguracionId { get; set; }

    /// <summary>
    /// Expresión CRON para definir la planificación.
    /// Ejemplo: "5 17 * * *" para programar la ejecución a las 17:05 todos los días.
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;
}