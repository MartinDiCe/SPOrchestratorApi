using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Models.Base;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services;

namespace SPOrchestratorAPI.Data;

public class ApplicationDbContext : DbContext
{
    private readonly AuditEntitiesService _auditEntitiesService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, AuditEntitiesService auditEntitiesService)
        : base(options)
    {
        _auditEntitiesService = auditEntitiesService;
    }
    
    public DbSet<Servicio>? Servicio { get; set; }

    public override int SaveChanges()
    {
        _auditEntitiesService.ApplyAudit(ChangeTracker.Entries<AuditEntities>());
        return base.SaveChanges();
    }
}