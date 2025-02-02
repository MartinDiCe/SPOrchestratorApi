namespace SPOrchestratorAPI.Models.DTOs;

public class ServicioConfiguracionDtoResponse
{
    public int Id { get; init; }
    public int ServicioId { get; init; }
    public string NombreProcedimiento { get; set; } = string.Empty;
    public string ConexionBaseDatos { get; set; } = string.Empty;
    public string Parametros { get; set; } = string.Empty;
    public int MaxReintentos { get; set; }
    public int TimeoutSegundos { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool? Deleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}