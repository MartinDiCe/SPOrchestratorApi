using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Services.LoggingServices;  // tu ILoggerService<T>
using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Extensiones para cargar la clave de licencia de New Relic desde la base de datos
    /// sin tirar de EF, y volcarla en IConfiguration en memoria.
    /// </summary>
    public static class NewRelicConfiguration
    {
        private const string ParameterName = "NewRelicLicenseKey";

        /// <summary>
        /// Registra los servicios mínimos, lee el parámetro de licencia
        /// y lo inyecta en IConfiguration.
        /// </summary>
        public static void AddNewRelicLicenseFromDatabase(this WebApplicationBuilder builder)
        {
            // 1) Registrar lo mínimo para leer parámetros y para loguear aquí
            builder.Services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));
            builder.Services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IParameterRepository, ParameterRepository>();

            // 2) Build provisional para leer la clave y obtener un logger
            using var tempProvider = builder.Services.BuildServiceProvider();
            var logger = tempProvider.GetService<ILoggerService<ParameterRepository>>();

            string? licenseKey = null;
            try
            {
                // 3) Lectura ADO.NET sencilla
                var connString = builder.Configuration.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
                    SELECT TOP 1 ParameterValue
                      FROM Parameter
                     WHERE ParameterName = @name";
                cmd.Parameters.AddWithValue("@name", ParameterName);

                var result = cmd.ExecuteScalar();
                licenseKey = result as string;
            }
            catch (Exception ex)
            {
                // 4) En lugar de ignorar, lo registramos
                logger?.LogWarning($"No se pudo leer NewRelicLicenseKey: {ex.Message}");
            }

            // 5) Si lo encontramos, lo inyectamos en la configuración
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                builder.Configuration
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["NewRelic:LicenseKey"] = licenseKey
                    });
                logger?.LogInfo("New Relic license key cargada correctamente desde la base de datos.");
            }
            else
            {
                logger?.LogInfo("New Relic license key no encontrada; se omite integración con New Relic Logs.");
            }
        }
    }
}
