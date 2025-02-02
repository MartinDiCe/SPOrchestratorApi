namespace SPOrchestratorAPI.Models.DTOs;

public class UpdateServicioConfiguracionDto
{
    public int Id { get; set; }  
    public int ServicioId { get; set; }  
    public string NombreProcedimiento { get; set; } = string.Empty;
    public string ConexionBaseDatos { get; set; } = string.Empty;
    public string Parametros { get; set; } = string.Empty;
    public int MaxReintentos { get; set; }
    public int TimeoutSegundos { get; set; }
}