using System.Data;
using System.Reactive.Linq;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Enums;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;

namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    /// <summary>
    /// Servicio para la ejecución de procedimientos almacenados (Stored Procedures) utilizando la configuración específica 
    /// definida en la entidad <see cref="ServicioConfiguracion"/>. Este servicio utiliza un enfoque reactivo y captura errores 
    /// a través de un <see cref="IServiceExecutor"/>.
    /// </summary>
    public class StoredProcedureService : IStoredProcedureService
    {
        private readonly ILoggerService<StoredProcedureService> _logger;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly IServicioConfiguracionService _configService;
        private readonly IServicioService _servicioService;

        /// <summary>
        /// Crea una nueva instancia de <see cref="StoredProcedureService"/>.
        /// </summary>
        /// <param name="configService">
        /// Servicio que permite obtener la configuración específica (que incluye el nombre del SP, cadena de conexión, proveedor y parámetros esperados)
        /// a partir de su ID.
        /// </param>
        /// <param name="logger">Servicio de logging para registrar información o errores.</param>
        /// <param name="serviceExecutor">Executor reactivo para la captura centralizada de excepciones.</param>
        /// <exception cref="ArgumentNullException">
        /// Se lanza si alguno de los parámetros es nulo.
        /// </exception>
        public StoredProcedureService(
            IServicioConfiguracionService configService,
            ILoggerService<StoredProcedureService> logger,
            IServiceExecutor serviceExecutor, IServicioService servicioService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
            _servicioService = servicioService;
        }

        /// <inheritdoc />
        public IObservable<int> EjecutarSpAsync(int idConfiguracion, IDictionary<string, object>? parameters = null)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Iniciando ejecución del stored procedure para la configuración con ID {idConfiguracion}.");
                    
                    var config = await _configService.GetByIdAsync(idConfiguracion).FirstAsync();
                    if (config == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró configuración con ID {idConfiguracion}.");
                    }

                    var spName = config.NombreProcedimiento;
                    if (string.IsNullOrWhiteSpace(spName))
                    {
                        throw new InvalidOperationException("El nombre del stored procedure no está definido en la configuración.");
                    }

                    _logger.LogInfo($"Se obtuvo la configuración. Proveedor: {config.Provider}, Cadena de conexión: {config.ConexionBaseDatos}, SP: {spName}");

                    // Si se definen parámetros esperados en la configuración, validar:
                    if (!string.IsNullOrWhiteSpace(config.Parametros))
                    {
                        Dictionary<string, string> expectedParams;
                        try
                        {
                            expectedParams = JsonSerializer.Deserialize<Dictionary<string, string>>(config.Parametros)
                                             ?? new Dictionary<string, string>();
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("No se pudo deserializar la configuración de parámetros esperados.", ex);
                        }

                        // Los valores (nombres de parámetros) esperados se extraen del JSON
                        var expectedValues = new HashSet<string>(expectedParams.Values, StringComparer.OrdinalIgnoreCase);
                        var missingParams = new List<string>();
                        var extraParams = new List<string>();

                        var parametersCi = parameters != null
                            ? new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase)
                            : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                        foreach (var expected in expectedValues)
                        {
                            if (!parametersCi.ContainsKey(expected))
                            {
                                missingParams.Add(expected);
                            }
                        }

                        foreach (var key in parametersCi.Keys)
                        {
                            if (!expectedValues.Contains(key))
                            {
                                extraParams.Add(key);
                            }
                        }

                        if (missingParams.Count > 0)
                        {
                            throw new ArgumentException($"Faltan los siguientes parámetros requeridos: {string.Join(", ", missingParams)}.");
                        }
                        if (extraParams.Count > 0)
                        {
                            throw new ArgumentException($"Se enviaron parámetros no esperados: {string.Join(", ", extraParams)}.");
                        }
                    }

                    int rowsAffected = 0;

                    switch (config.Provider)
                    {
                        case DatabaseProvider.SqlServer:
                            using (var connection = new SqlConnection(config.ConexionBaseDatos))
                            {
                                await connection.OpenAsync();
                                _logger.LogInfo($"Conexión abierta a la base de datos: {config.ConexionBaseDatos}");
                                using (var command = new SqlCommand(spName, connection)
                                {
                                    CommandType = CommandType.StoredProcedure
                                })
                                {
                                    // Agregar solo los parámetros enviados (ya se validó que sean exactamente los esperados)
                                    if (parameters != null && parameters.Count > 0)
                                    {
                                        foreach (var kvp in parameters)
                                        {
                                            var paramName = kvp.Key; // Se usa el nombre tal cual se recibe
                                            object paramValue = kvp.Value ?? DBNull.Value;

                                            // Si el valor es un JsonElement, extraer su valor nativo
                                            if (paramValue is JsonElement jsonElem)
                                            {
                                                switch (jsonElem.ValueKind)
                                                {
                                                    case JsonValueKind.String:
                                                        {
                                                            string? valueStr = jsonElem.GetString();
                                                            paramValue = valueStr != null ? (object)valueStr : DBNull.Value;
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

                                    rowsAffected = await command.ExecuteNonQueryAsync();

                                    if (rowsAffected == -1)
                                    {
                                        _logger.LogInfo("El stored procedure devolvió -1, lo que se interpreta como ejecución exitosa con resultados.");
                                    }
                                }
                            }
                            break;

                        default:
                            throw new NotSupportedException("Proveedor de base de datos no soportado para ejecución de SP.");
                    }

                    _logger.LogInfo($"Se ejecutó el SP '{spName}' correctamente. Filas afectadas (o -1 si se retornaron resultados): {rowsAffected}.");
                    return rowsAffected;
                });
            });
        }
        
        /// <inheritdoc />
        public IObservable<object> EjecutarSpConRespuestaPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Iniciando ejecución del stored procedure para el servicio '{serviceName}'.");

                    // Buscar el servicio por su nombre
                    var servicio = await _servicioService.GetByNameAsync(serviceName).FirstAsync();
                    if (servicio == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");
                    }

                    // Obtener la configuración asociada al servicio (usamos la primera configuración encontrada)
                    var configs = await _configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                    if (configs == null || configs.Count == 0)
                    {
                        throw new ResourceNotFoundException($"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");
                    }
                    var config = configs[0];

                    // Extraer el nombre del SP desde la configuración
                    var spName = config.NombreProcedimiento;
                    if (string.IsNullOrWhiteSpace(spName))
                    {
                        throw new InvalidOperationException("El nombre del stored procedure no está definido en la configuración.");
                    }

                    _logger.LogInfo($"Se obtuvo la configuración para el servicio '{serviceName}'. Proveedor: {config.Provider}, Cadena de conexión: {config.ConexionBaseDatos}, SP: {spName}");

                    // Validar que se envíen exactamente los parámetros esperados, si se han definido en la configuración.
                    if (!string.IsNullOrWhiteSpace(config.Parametros))
                    {
                        Dictionary<string, string> expectedParams;
                        try
                        {
                            expectedParams = JsonSerializer.Deserialize<Dictionary<string, string>>(config.Parametros)
                                             ?? new Dictionary<string, string>();
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("No se pudo deserializar la configuración de parámetros esperados.", ex);
                        }

                        // Extraer los valores esperados (nombres de parámetros) de forma case-insensitive.
                        var expectedValues = new HashSet<string>(expectedParams.Values, StringComparer.OrdinalIgnoreCase);
                        var missingParams = new List<string>();
                        var extraParams = new List<string>();

                        var parametersCI = parameters != null
                            ? new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase)
                            : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                        // Validar que no falten parámetros
                        foreach (var expected in expectedValues)
                        {
                            if (!parametersCI.ContainsKey(expected))
                            {
                                missingParams.Add(expected);
                            }
                        }

                        // Validar que no se hayan enviado parámetros extra
                        foreach (var key in parametersCI.Keys)
                        {
                            if (!expectedValues.Contains(key))
                            {
                                extraParams.Add(key);
                            }
                        }

                        if (missingParams.Count > 0)
                        {
                            throw new ArgumentException($"Faltan los siguientes parámetros requeridos: {string.Join(", ", missingParams)}.");
                        }
                        if (extraParams.Count > 0)
                        {
                            throw new ArgumentException($"Se enviaron parámetros no esperados: {string.Join(", ", extraParams)}.");
                        }
                    }

                    object resultData;

                    // Ejecutar el SP según el proveedor (implementado para SQL Server)
                    switch (config.Provider)
                    {
                        case DatabaseProvider.SqlServer:
                            using (var connection = new SqlConnection(config.ConexionBaseDatos))
                            {
                                await connection.OpenAsync();
                                _logger.LogInfo($"Conexión abierta a la base de datos: {config.ConexionBaseDatos}");
                                using (var command = new SqlCommand(spName, connection)
                                {
                                    CommandType = CommandType.StoredProcedure
                                })
                                {
                                    // Agregar los parámetros (ya validados)
                                    if (parameters != null && parameters.Count > 0)
                                    {
                                        foreach (var kvp in parameters)
                                        {
                                            var paramName = kvp.Key;
                                            object paramValue = kvp.Value ?? DBNull.Value;

                                            // Convertir JsonElement a su valor nativo, si corresponde
                                            if (paramValue is JsonElement jsonElem)
                                            {
                                                switch (jsonElem.ValueKind)
                                                {
                                                    case JsonValueKind.String:
                                                        {
                                                            string? valueStr = jsonElem.GetString();
                                                            paramValue = valueStr != null ? (object)valueStr : DBNull.Value;
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

                                    // Ejecutar el SP y capturar los resultados usando un DataReader
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
                            break;

                        default:
                            throw new NotSupportedException("Proveedor de base de datos no soportado para ejecución de SP.");
                    }

                    _logger.LogInfo("El stored procedure se ejecutó correctamente y se obtuvo la respuesta.");
                    return resultData;
                });
            });
        }
    }
}
