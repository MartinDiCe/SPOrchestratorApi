using System.Reflection;
using Microsoft.OpenApi.Models;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Clase estática que agrega y configura Swagger 
    /// para documentar la API.
    /// </summary>
    public static class SwaggerConfig
    {
        /// <summary>
        /// Agrega los servicios de Swagger y Endpoints de API a la colección de servicios,
        /// configurando la documentación básica.
        /// </summary>
        /// <param name="services">
        /// Colección de servicios de la aplicación.
        /// </param>
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Documento base (v1)
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SPOrchestratorAPI",
                    Version = "0.0.0.1",
                    Description = "API para la ejecución de Stored Procedures dinámicos",
                    Contact = new OpenApiContact
                    {
                        Name = "Mdice",
                        Email = "mdice@diceprojects.com",
                        Url = new Uri("https://github.com/MartinDiCe/SPOrchestratorApi")
                    }
                });

                // Incluir comentarios XML si están habilitados en el proyecto
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });
        }

        /// <summary>
        /// Activa Swagger y su interfaz de usuario en tiempo de ejecución,
        /// comúnmente usado en entornos de desarrollo.
        /// </summary>
        /// <param name="app">
        /// Instancia de <see cref="WebApplication"/> que define el pipeline de la aplicación.
        /// </param>
        public static void UseSwaggerConfiguration(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    // Indica la ruta del archivo swagger.json
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SPOrchestratorAPI v1");
                    // Define el path donde se sirve la UI de Swagger
                    options.RoutePrefix = "swagger";
                });
            }
        }
    }
}