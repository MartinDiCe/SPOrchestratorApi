using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Models.Entities;

[Table("Servicio")]
public class Servicio : AuditEntities
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public bool Status { get; set; } = true;
    
}