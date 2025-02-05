using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SPOrchestratorAPI.Models.Base;

namespace SPOrchestratorAPI.Services
{
    /// <summary>
    /// Servicio que aplica la lógica de auditoría sobre entidades que heredan de <see cref="AuditEntities"/>.
    /// Asigna valores para CreatedAt, UpdatedAt, DeletedAt, etc., basándose en el estado de la entidad.
    /// </summary>
    public class AuditEntitiesService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor de la clase <see cref="AuditEntitiesService"/>.
        /// </summary>
        /// <param name="httpContextAccessor">
        /// Acceso para el contexto HTTP, usado para obtener el nombre del usuario (si está autenticado).
        /// </param>
        /// <exception cref="ArgumentNullException">Si <paramref name="httpContextAccessor"/> es nulo.</exception>
        public AuditEntitiesService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Aplica valores de auditoría (fechas y usuario) a las entidades en función de su estado en el contexto de EF.
        /// </summary>
        /// <param name="entries">
        /// Colección de <see cref="EntityEntry{TEntity}"/> que hacen referencia a entidades de tipo <see cref="AuditEntities"/>.
        /// Se analiza cada elemento para asignar los campos de auditoría según su <see cref="EntityState"/>.
        /// </param>
        public void ApplyAudit(IEnumerable<EntityEntry<AuditEntities>> entries)
        {
            
            var userName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "System";

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = userName;
                        break;

                    case EntityState.Modified:
                        
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = userName;
                        break;

                    case EntityState.Deleted:

                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.DeletedBy = userName;
                        entry.Entity.Deleted = true;

                        entry.State = EntityState.Modified;
                        break;

                }
            }
        }
    }
}
