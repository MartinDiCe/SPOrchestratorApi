using Microsoft.Extensions.Configuration;
using System;

namespace SPOrchestratorAPI.Configuration;

/// <summary>
/// Clase que proporciona la cadena de conexión a la base de datos,
/// obtenida desde la configuración de la aplicación (appsettings, por ejemplo).
/// </summary>
public class DatabaseConfig
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor de <see cref="DatabaseConfig"/>, que recibe la configuración general de la aplicación.
    /// </summary>
    /// <param name="configuration">
    /// Objeto que proporciona acceso a los valores de configuración (por ejemplo, las secciones de appsettings.json).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Se lanza si <paramref name="configuration"/> es <c>null</c>.
    /// </exception>
    public DatabaseConfig(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Obtiene la cadena de conexión denominada "DefaultConnection" 
    /// desde la configuración de la aplicación.
    /// </summary>
    /// <returns>
    /// La cadena de conexión asociada a "DefaultConnection".
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si no se encuentra la cadena de conexión en la configuración.
    /// </exception>
    public string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
    }
}