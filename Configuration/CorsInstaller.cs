using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Extensiones para configurar CORS dinámico
    /// leyendo el toggle y la lista de orígenes desde la tabla Parameter.
    /// </summary>
    public static class CorsInstaller
    {
        private const string ToggleName  = "CorsEnabled";
        private const string OriginsName = "CorsAllowedOrigins";
        private const string PolicyName  = "DynamicCorsPolicy";

        /// <summary>
        /// Lee el parámetro <c>CorsEnabled</c> y <c>CorsAllowedOrigins</c> de la base
        /// (suponiendo que ya existen o fueron seed-eados), y registra la política
        /// de CORS si está habilitado.
        /// </summary>
        public static async Task<WebApplicationBuilder> AddDynamicCorsAsync(
            this WebApplicationBuilder builder)
        {
            // Resolver el repositorio desde el contenedor
            await using var sp   = builder.Services.BuildServiceProvider();
            var         repo     = sp.GetRequiredService<IParameterRepository>();

            // 1) Leer toggle
            var toggleParam = await repo.GetByNameAsync(ToggleName);
            bool enabled = toggleParam?.ParameterValue?
                               .Equals("true", StringComparison.OrdinalIgnoreCase)
                           ?? false;

            if (!enabled)
            {
                return builder;
            }

            // 2) Leer orígenes
            var originsParam = await repo.GetByNameAsync(OriginsName);
            var origins = originsParam?.ParameterValue?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToArray()
                ?? Array.Empty<string>();

            if (!origins.Any())
            {
                return builder;
            }

            // 3) Registrar la política de CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy =>
                {
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return builder;
        }

        /// <summary>
        /// Inserta en el pipeline el middleware de CORS usando la política dinámica
        /// y devuelve el mismo <see cref="WebApplication"/> para encadenar.
        /// </summary>
        public static WebApplication UseDynamicCors(this WebApplication app)
        {
            app.UseCors(PolicyName);
            return app;
        }
        
    }
}
