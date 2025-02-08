using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Models.Base;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.AuditServices;

namespace SPOrchestratorAPI.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    AuditEntitiesService auditEntitiesService)
    : DbContext(options)
{
    public DbSet<Servicio>? Servicio { get; set; }
    public DbSet<ServicioConfiguracion>? ServicioConfiguracion { get; set; }
    public DbSet<Parameter> Parameters { get; set; } = null!;
    public DbSet<ApiTrace> ApiTraces { get; set; } = null!;

    public override int SaveChanges()
    {
        auditEntitiesService.ApplyAudit(ChangeTracker.Entries<AuditEntities>());
        return base.SaveChanges();
    }
}