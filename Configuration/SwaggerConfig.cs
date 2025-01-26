using System.Reflection;
using Microsoft.OpenApi.Models;

namespace SPOrchestratorAPI.Configuration;

public static class SwaggerConfig
{
    public static void AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
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

            // Habilitar XML Comments si están activados en el csproj
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
    }

    public static void UseSwaggerConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SPOrchestratorAPI v1");
                options.RoutePrefix = string.Empty; 
            });
        }
    }
}