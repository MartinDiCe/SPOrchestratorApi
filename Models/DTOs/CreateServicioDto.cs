namespace SPOrchestratorAPI.Models.DTOs;

public class CreateServicioDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
}