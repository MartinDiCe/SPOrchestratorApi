using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.Base;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.AuditServices;

namespace SPOrchestratorAPI.Data
{
    /// <summary>
    /// Representa el contexto de datos principal de la aplicación, 
    /// encargado de administrar la conexión a la base de datos y la configuración de las entidades.
    /// </summary>
    /// <param name="options">Opciones de configuración para el DbContext.</param>
    /// <param name="auditEntitiesService">
    /// Servicio que se encarga de aplicar auditoría a las entidades derivadas de <see cref="AuditEntities"/>.
    /// </param>
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditEntitiesService auditEntitiesService)
        : DbContext(options)
    {
        /// <summary>
        /// Conjunto de datos para la entidad <see cref="Servicio"/>.
        /// </summary>
        public DbSet<Servicio>? Servicio { get; set; }

        /// <summary>
        /// Conjunto de datos para la entidad <see cref="ServicioConfiguracion"/>.
        /// </summary>
        public DbSet<ServicioConfiguracion>? ServicioConfiguracion { get; set; }

        /// <summary>
        /// Conjunto de datos para la entidad <see cref="Parameter"/>.
        /// </summary>
        public DbSet<Parameter> Parameters { get; set; } = null!;

        /// <summary>
        /// Conjunto de datos para la entidad <see cref="ApiTrace"/>.
        /// </summary>
        public DbSet<ApiTrace> ApiTraces { get; set; } = null!;

        /// <summary>
        /// Conjunto de datos para la entidad <see cref="ServicioProgramacion"/>.
        /// </summary>
        public DbSet<ServicioProgramacion> ServicioProgramacion { get; set; } = null!;
        
        /// <summary>
        /// Conjunto de datos para la entidad <see cref="ServicioEjecucion"/>.
        /// </summary>
        public DbSet<ServicioEjecucion> ServicioEjecucion { get; set; } = null!;
        
        /// <summary>
        /// Conjunto de datos para la entidad <see cref="ServicioContinueWiths"/>.
        /// </summary>
        public DbSet<ServicioContinueWith> ServicioContinueWiths { get; set; } = null!;

        
        /// <summary>
        /// Guarda los cambios en la base de datos, aplicando previamente la auditoría a las entidades modificadas.
        /// </summary>
        /// <returns>El número de registros afectados.</returns>
        public override int SaveChanges()
        {
            auditEntitiesService.ApplyAudit(ChangeTracker.Entries<AuditEntities>());
            return base.SaveChanges();
        }

        /// <summary>
        /// Configura el modelo de datos para la aplicación.
        /// Se utiliza, por ejemplo, para aplicar un value converter que cifra la propiedad <c>ConexionBaseDatos</c>
        /// de la entidad <see cref="ServicioConfiguracion"/>, protegiendo así las credenciales almacenadas.
        /// </summary>
        /// <param name="modelBuilder">El builder para configurar el modelo.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServicioConfiguracion>()
                .Property(sc => sc.ConexionBaseDatos)
                .HasConversion(new EncryptionConverter());

            modelBuilder.Entity<ServicioEjecucion>()
                .HasOne(e => e.ServicioEjecucionDesencadenador)         
                .WithMany(p => p.Hijos)                                  
                .HasForeignKey(e => e.ServicioEjecucionDesencadenadorId) 
                .OnDelete(DeleteBehavior.Restrict);                     

            modelBuilder.Entity<ServicioEjecucion>()
                .HasIndex(e => e.ServicioEjecucionDesencadenadorId);
        }
    }
}
