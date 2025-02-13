using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    /// <summary>
    /// Implementación del executor para SQL Server.
    /// Proporciona métodos para ejecutar stored procedures de forma asíncrona,
    /// tanto para operaciones sin retorno de datos (NonQuery) como para aquellas que retornan datos.
    /// </summary>
    public class SqlServerStoredProcedureExecutor(ILoggerService<SqlServerStoredProcedureExecutor> logger)
        : IStoredProcedureExecutor
    {
        private readonly ILoggerService<SqlServerStoredProcedureExecutor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Ejecuta un stored procedure de SQL Server sin retornar datos (operación NonQuery).
        /// </summary>
        /// <param name="config">
        /// La configuración del stored procedure que incluye la cadena de conexión, el nombre del SP, entre otros.
        /// </param>
        /// <param name="parameters">
        /// Diccionario opcional de parámetros a enviar al stored procedure. Las claves deben corresponder
        /// a los nombres de los parámetros definidos en el SP.
        /// </param>
        /// <returns>
        /// Un entero que indica el número de filas afectadas por la ejecución del SP. Si el SP retorna -1,
        /// se interpreta como ejecución exitosa con resultados.
        /// </returns>
        public async Task<int> ExecuteNonQueryAsync(ServicioConfiguracion config, IDictionary<string, object>? parameters)
        {
            int rowsAffected = 0;
            using (var connection = new SqlConnection(config.ConexionBaseDatos))
            {
                await connection.OpenAsync();
                _logger.LogInfo($"Conexión abierta a la base de datos: {config.ConexionBaseDatos}");
                using (var command = new SqlCommand(config.NombreProcedimiento, connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    AddParameters(command, parameters, config.NombreProcedimiento);
                    rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == -1)
                    {
                        _logger.LogInfo("El stored procedure devolvió -1, lo que se interpreta como ejecución exitosa con resultados.");
                    }
                }
            }
            _logger.LogInfo($"Se ejecutó el SP '{config.NombreProcedimiento}' correctamente. Filas afectadas: {rowsAffected}");
            return rowsAffected;
        }

        /// <summary>
        /// Ejecuta un stored procedure de SQL Server y retorna los datos obtenidos.
        /// </summary>
        /// <param name="config">
        /// La configuración del stored procedure que incluye la cadena de conexión, el nombre del SP, entre otros.
        /// </param>
        /// <param name="parameters">
        /// Diccionario opcional de parámetros a enviar al stored procedure. Las claves deben corresponder
        /// a los nombres de los parámetros definidos en el SP.
        /// </param>
        /// <returns>
        /// Un objeto que contiene la respuesta del SP, típicamente una lista de diccionarios donde cada uno representa una fila.
        /// </returns>
        public async Task<object> ExecuteReaderAsync(ServicioConfiguracion config, IDictionary<string, object>? parameters)
        {
            object resultData;
            using (var connection = new SqlConnection(config.ConexionBaseDatos))
            {
                await connection.OpenAsync();
                _logger.LogInfo($"Conexión abierta a la base de datos: {config.ConexionBaseDatos}");
                using (var command = new SqlCommand(config.NombreProcedimiento, connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    AddParameters(command, parameters, config.NombreProcedimiento);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var resultList = new List<Dictionary<string, object>>();
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var columnName = reader.GetName(i);
                                var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                                row[columnName] = value!;
                            }
                            resultList.Add(row);
                        }
                        resultData = resultList;
                    }
                }
            }
            _logger.LogInfo($"El SP '{config.NombreProcedimiento}' se ejecutó correctamente y se obtuvo la respuesta.");
            return resultData;
        }

        /// <summary>
        /// Agrega los parámetros al objeto <see cref="SqlCommand"/> que se utilizará para ejecutar el SP.
        /// Realiza la conversión de valores de tipo <see cref="JsonElement"/> a su tipo nativo.
        /// </summary>
        /// <param name="command">El objeto <see cref="SqlCommand"/> al que se agregarán los parámetros.</param>
        /// <param name="parameters">
        /// Diccionario de parámetros a enviar al SP. Las claves son los nombres de los parámetros.
        /// </param>
        /// <param name="spName">El nombre del stored procedure (usado para el logging).</param>
        private void AddParameters(SqlCommand command, IDictionary<string, object>? parameters, string spName)
        {
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var kvp in parameters)
                {
                    var paramName = kvp.Key;
                    object paramValue = kvp.Value ?? DBNull.Value;
                    
                    if (paramValue is JsonElement jsonElem)
                    {
                        switch (jsonElem.ValueKind)
                        {
                            case JsonValueKind.String:
                                {
                                    string? valueStr = jsonElem.GetString();
                                    paramValue = string.IsNullOrWhiteSpace(valueStr) ? DBNull.Value : (object)valueStr;
                                    break;
                                }
                            case JsonValueKind.Number:
                                paramValue = jsonElem.GetRawText();
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                paramValue = jsonElem.GetBoolean();
                                break;
                            default:
                                paramValue = jsonElem.GetRawText();
                                break;
                        }
                    }
                    command.Parameters.AddWithValue(paramName, paramValue);
                }
                _logger.LogInfo($"Se agregaron {parameters.Count} parámetros al SP '{spName}'.");
            }
        }
    }
}
