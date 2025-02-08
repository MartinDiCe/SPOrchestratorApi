using SPOrchestratorAPI.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SPOrchestratorAPI.Configuration;

/// <summary>
/// Proporciona métodos para inicializar la base de datos 
/// (por ejemplo, crearla si no existe o aplicar migraciones).
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Inicializa la base de datos, creando un <see cref="IServiceScope"/>
    /// temporal para obtener el <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="serviceProvider">
    /// Proveedor de servicios que permite la resolución de dependencias en el entorno de la aplicación.
    /// </param>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        // Se crea un scope para garantizar que el DbContext se libere después de usarse.
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Crea la base de datos si no existe, sin aplicar migraciones.
        dbContext.Database.EnsureCreated();
    }
}