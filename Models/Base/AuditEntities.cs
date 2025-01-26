using System;
using System.ComponentModel.DataAnnotations;

namespace SPOrchestratorAPI.Models.Base;

public abstract class AuditEntities
{
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string CreatedBy { get; set; } = "System"; 

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }  

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }  

    [Required]
    public bool Deleted { get; set; } = false;
}