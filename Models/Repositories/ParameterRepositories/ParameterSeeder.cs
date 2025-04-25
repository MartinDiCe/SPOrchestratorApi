using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ParameterRepositories
{
    /// <summary>
    /// Clase encargada de inicializar parámetros por defecto.
    /// </summary>
    public static class ParameterSeeder
    {
        /// <summary>
        /// Se ejecuta para verificar la existencia de parámetros por defecto y crearlos en caso de que no existan.
        /// </summary>
        /// <param name="parameterRepository">Repositorio de parámetros.</param>
        public static async Task SeedDefaultParametersAsync(IParameterRepository parameterRepository)
        {

            const string timeoutParameterName = "TimeoutGlobal";
            var timeoutParameter = await parameterRepository.GetByNameAsync(timeoutParameterName);
            if (timeoutParameter == null)
            {
                var newTimeoutParameter = new Parameter
                {
                    ParameterName = timeoutParameterName,
                    ParameterValue = "30",
                    ParameterDescription = "Tiempo de espera global en segundos para el sistema",
                    ParameterCategory = "Sistema",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await parameterRepository.CreateAsync(newTimeoutParameter);
            }

            const string ambienteParameterName = "IdentificacionAmbiente";
            var ambienteParameter = await parameterRepository.GetByNameAsync(ambienteParameterName);
            if (ambienteParameter == null)
            {
                var newAmbienteParameter = new Parameter
                {
                    ParameterName = ambienteParameterName,
                    ParameterValue = "APIOrchestrator",
                    ParameterDescription = "Identificación del ambiente de la aplicación",
                    ParameterCategory = "Sistema",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await parameterRepository.CreateAsync(newAmbienteParameter);
            }

            const string apiTraceEnabledParameterName = "ApiTraceEnabled";
            var apiTraceEnabledParameter = await parameterRepository.GetByNameAsync(apiTraceEnabledParameterName);
            if (apiTraceEnabledParameter == null)
            {
                var newApiTraceEnabledParameter = new Parameter
                {
                    ParameterName = apiTraceEnabledParameterName,
                    ParameterValue = "true",
                    ParameterDescription = "Determina si se debe registrar la traza de la API",
                    ParameterCategory = "Sistema",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await parameterRepository.CreateAsync(newApiTraceEnabledParameter);
            }

            const string refreshCronParam = "JobsRefreshCron";
            var refreshCron = await parameterRepository.GetByNameAsync(refreshCronParam);
            if (refreshCron == null)
            {
                var newRefreshCron = new Parameter
                {
                    ParameterName = refreshCronParam,
                    ParameterValue = "0 */3 * * *",
                    ParameterDescription = "CRON para refrescar recurring jobs",
                    ParameterCategory = "Hangfire",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await parameterRepository.CreateAsync(newRefreshCron);
            }
            
            const string hangfireEnabledName = "HangfireEnabled";
            var hangfireEnabled = await parameterRepository.GetByNameAsync(hangfireEnabledName);
            if (hangfireEnabled == null)
            {
                var newHangfireEnabled = new Parameter
                {
                    ParameterName = hangfireEnabledName,
                    ParameterValue = "true",
                    ParameterDescription = "Determina si se habilita Hangfire (dashboard y jobs)",
                    ParameterCategory = "Hangfire",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                await parameterRepository.CreateAsync(newHangfireEnabled);
            }
            
            const string swaggerEnabledName = "SwaggerEnabled";
            var swaggerEnabled = await parameterRepository.GetByNameAsync(swaggerEnabledName);
            if (swaggerEnabled == null)
            {
                var newSwaggerEnabled = new Parameter
                {
                    ParameterName = swaggerEnabledName,
                    ParameterValue = "true",
                    ParameterDescription = "Determina si se habilita Swagger/OpenAPI UI",
                    ParameterCategory = "Swagger",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                await parameterRepository.CreateAsync(newSwaggerEnabled);
            }
            
        }
    }
}