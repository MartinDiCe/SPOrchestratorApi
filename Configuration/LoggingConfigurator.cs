using Serilog;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Configura el sistema de logging de la aplicación, incluyendo Serilog y
    /// opcionalmente el sink de New Relic Logs si existe una clave de licencia.
    /// </summary>
    public static class LoggingConfigurator
    {
        /// <summary>
        /// Inicializa y aplica la configuración de Serilog como proveedor de
        /// <see cref="Microsoft.Extensions.Logging"/> para el host de la aplicación.
        /// </summary>
        /// <param name="builder">
        /// <see cref="WebApplicationBuilder"/> con la configuración y el entorno
        /// de la aplicación donde se inyectará el logger.
        /// </param>
        public static void Configure(WebApplicationBuilder builder)
        {
            // Lee la clave de licencia de New Relic desde la configuración
            var licenseKey = builder.Configuration["NewRelic:LicenseKey"];

            // 1) Configuración base: volcar logs a consola
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console();

            // 2) Si existe clave de New Relic, añade el sink de New Relic Logs
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                loggerConfig = loggerConfig
                    .WriteTo.NewRelicLogs(
                        applicationName: builder.Environment.ApplicationName,
                        licenseKey: licenseKey
                    );
            }

            // 3) Crear el logger global y registrar Serilog como proveedor
            Log.Logger = loggerConfig.CreateLogger();
            builder.Host.UseSerilog();
        }
    }
}