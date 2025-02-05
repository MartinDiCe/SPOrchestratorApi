using Microsoft.Data.SqlClient;
using System.Data;
using System.Reactive.Linq;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Services.Logging;
using SPOrchestratorAPI.Exceptions; // por si deseas lanzar excepciones propias

namespace SPOrchestratorAPI.Services
{
    /// <summary>
    /// Servicio para la ejecución de procedimientos almacenados (Stored Procedures) en una base de datos SQL,
    /// con un enfoque reactivo y capturando errores a través de <see cref="IServiceExecutor"/>.
    /// </summary>
    public class StoredProcedureService
    {
        private readonly string _connectionString;
        private readonly ILoggerService<StoredProcedureService> _logger;
        private readonly IServiceExecutor _serviceExecutor;

        /// <summary>
        /// Crea una nueva instancia de <see cref="StoredProcedureService"/> utilizando la configuración de conexión especificada,
        /// un logger y un executor reactivo para la captura centralizada de excepciones.
        /// </summary>
        /// <param name="dbConfig">Configuración que provee la cadena de conexión de la base de datos.</param>
        /// <param name="logger">Servicio de logging para registrar información o errores.</param>
        /// <param name="serviceExecutor">Ejecutor reactivo para manejar la ejecución y captura de errores.</param>
        /// <exception cref="ArgumentNullException">
        /// Se lanza si alguno de los parámetros <paramref name="dbConfig"/>, <paramref name="logger"/> 
        /// o <paramref name="serviceExecutor"/> es nulo.
        /// </exception>
        public StoredProcedureService(
            DatabaseConfig dbConfig,
            ILoggerService<StoredProcedureService> logger,
            IServiceExecutor serviceExecutor)
        {
            if (dbConfig == null) 
                throw new ArgumentNullException(nameof(dbConfig));
            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
            
            _connectionString = dbConfig.GetConnectionString();
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado de manera reactiva.
        /// </summary>
        /// <param name="spName">Nombre del procedimiento almacenado a ejecutar.</param>
        /// <param name="parameters">
        /// Diccionario opcional que contiene pares (nombreParámetro, valorParámetro). 
        /// Si es <c>null</c> o está vacío, no se agregan parámetros al SP.
        /// </param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite un <see cref="int"/> indicando cuántas filas fueron afectadas 
        /// por la ejecución (según la convención de <see cref="SqlCommand.ExecuteNonQuery()"/>).
        /// </returns>
        /// <remarks>
        /// Para procedimientos sin parámetros, se puede invocar sin el argumento <paramref name="parameters"/> 
        /// o con un diccionario vacío.
        /// </remarks>
        public IObservable<int> EjecutarSpAsync(string spName, IDictionary<string, object>? parameters = null)
        {
            
            return _serviceExecutor.ExecuteAsync(() =>
            {
                
                return Observable.FromAsync(async () =>
                {
                    if (string.IsNullOrWhiteSpace(spName))
                    {
                        _logger.LogWarning("El nombre del stored procedure no puede ser nulo o vacío.");
                        throw new ArgumentException("El nombre del stored procedure es obligatorio.", nameof(spName));
                    }

                    _logger.LogInfo($"Iniciando ejecución del stored procedure '{spName}'.");

                    // Abrimos conexión con ADO.NET
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();

                    // Construimos el comando para llamar al SP
                    using var command = new SqlCommand(spName, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Si hay parámetros, los agregamos
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (var kvp in parameters)
                        {
                            var paramName = kvp.Key;
                            var paramValue = kvp.Value ?? DBNull.Value;

                            command.Parameters.AddWithValue(paramName, paramValue);
                        }
                        _logger.LogInfo($"Se agregaron {parameters.Count} parámetros al SP '{spName}'.");
                    }

                    // Ejecutamos el SP y obtenemos el número de filas afectadas
                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    _logger.LogInfo($"Se ejecutó el SP '{spName}' correctamente. Filas afectadas: {rowsAffected}.");
                    return rowsAffected;
                });
            });
        }
    }
}
