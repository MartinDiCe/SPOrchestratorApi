using Hangfire;
using Hangfire.SqlServer;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Proporciona métodos de extensión para configurar y montar Hangfire en la aplicación.
    /// </summary>
    public static class HangfireInstaller
    {
        /// <summary>
        /// Registra los servicios de Hangfire (almacenamiento y servidor) en el contenedor de dependencias.
        /// </summary>
        /// <param name="services">Colección de servicios donde se añaden los servicios de Hangfire.</param>
        /// <param name="config">Configuración de la aplicación para leer cadenas de conexión y opciones.</param>
        /// <returns>La misma colección de servicios, para encadenamiento.</returns>
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");

            services.AddHangfire(cfg =>
            {
                cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                   .UseSimpleAssemblyNameTypeSerializer()
                   .UseRecommendedSerializerSettings()
                   .UseSqlServerStorage(conn, new SqlServerStorageOptions
                   {
                       CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                       SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                       QueuePollInterval            = TimeSpan.Zero,
                       UseRecommendedIsolationLevel = true,
                       DisableGlobalLocks           = true
                   });
            });

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
                options.Queues      = new[] { "default" };
            });

            return services;
        }
    }
}
