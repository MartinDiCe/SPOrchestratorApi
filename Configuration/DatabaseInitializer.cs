using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;

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
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Database.EnsureCreated();
        
        var parameterRepository = scope.ServiceProvider.GetRequiredService<IParameterRepository>();
        
        ParameterSeeder.SeedDefaultParametersAsync(parameterRepository).GetAwaiter().GetResult();
        
    }
}