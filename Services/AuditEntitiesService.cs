using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Services;

public class AuditEntitiesService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditEntitiesService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void ApplyAudit(IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditEntities>> entries)
    {
        var userName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "System";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = userName;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userName;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.DeletedBy = userName;
                entry.State = EntityState.Modified;
                entry.Entity.Deleted = true;
            }
        }
    }
}