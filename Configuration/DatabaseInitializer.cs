using Microsoft.Data.SqlClient;
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
        // ----------------------------------------------------------------
        // 1) Obtener connection string y ajustarla para apuntar a master
        // ----------------------------------------------------------------

        // Creamos un scope temporal para resolver DatabaseConfig
        using var scopeForMaster = serviceProvider.CreateScope();
        var config = scopeForMaster.ServiceProvider.GetRequiredService<DatabaseConfig>();

        // Reemplazamos en la cadena "Database=SPOrchestratorDb" por "Database=master"
        var masterCs = config
            .GetConnectionString()
            .Replace("Database=SPOrchestratorDb;", "Database=master;");

        // ----------------------------------------------------------------
        // 2) Crear la base SPOrchestratorDb si no existe
        // ----------------------------------------------------------------

        using (var masterConn = new SqlConnection(masterCs))
        {
            masterConn.Open();
            using var cmd = masterConn.CreateCommand();

            // Solo crea la base si no existe ya en sys.databases
            cmd.CommandText = @"
                    IF NOT EXISTS (
                      SELECT name 
                        FROM sys.databases 
                       WHERE name = N'SPOrchestratorDb')
                    BEGIN
                      CREATE DATABASE SPOrchestratorDb;
                    END";

            cmd.ExecuteNonQuery();
        }

        using var scopeForApp = serviceProvider.CreateScope();

        var dbContext = scopeForApp.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();

        var parameterRepository = scopeForApp.ServiceProvider.GetRequiredService<IParameterRepository>();
        ParameterSeeder
            .SeedDefaultParametersAsync(parameterRepository)
            .GetAwaiter()
            .GetResult();
    }
}