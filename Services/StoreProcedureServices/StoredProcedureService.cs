using System;
using System.Collections.Generic;
using System.Data;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Enums;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;

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
            IServiceExecutor serviceExecutor)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado utilizando la configuración identificada por <paramref name="idConfiguracion"/>.
        /// El request solo debe incluir el ID de configuración y los valores de los parámetros; el nombre del SP,
        /// la cadena de conexión, el proveedor y los parámetros esperados se obtienen de la entidad <see cref="ServicioConfiguracion"/>.
        /// Además, se valida que se envíen exactamente los parámetros esperados (sin parámetros extra).
        /// Si el SP devuelve -1 (lo que suele ocurrir cuando retorna un conjunto de resultados o no reporta filas afectadas),
        /// se interpreta como ejecución exitosa.
        /// </summary>
        /// <param name="idConfiguracion">
        /// Identificador de la configuración (<see cref="ServicioConfiguracion"/>) que contiene la información para ejecutar el SP.
        /// </param>
        /// <param name="parameters">
        /// Diccionario opcional con los valores de los parámetros a enviar. Si es null o está vacío, se ejecuta sin parámetros.
        /// </param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite un <see cref="int"/> indicando el número de filas afectadas 
        /// (o -1 si se retorna -1, lo que se interpreta como OK).
        /// </returns>
        public IObservable<int> EjecutarSpAsync(int idConfiguracion, IDictionary<string, object>? parameters = null)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Iniciando ejecución del stored procedure para la configuración con ID {idConfiguracion}.");

                    // Recuperar la configuración específica
                    var config = await _configService.GetByIdAsync(idConfiguracion).FirstAsync();
                    if (config == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró configuración con ID {idConfiguracion}.");
                    }

                    // Se obtiene el nombre del SP desde la configuración (ignorando cualquier valor que venga en el request)
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

                        // Convertir los parámetros enviados a un diccionario case-insensitive
                        var parametersCI = parameters != null
                            ? new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase)
                            : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                        // Verificar que falten parámetros
                        foreach (var expected in expectedValues)
                        {
                            if (!parametersCI.ContainsKey(expected))
                            {
                                missingParams.Add(expected);
                            }
                        }

                        // Verificar que no se hayan enviado parámetros extra
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

                    int rowsAffected = 0;

                    // Ejecutar el SP según el proveedor; en este ejemplo se implementa el caso de SQL Server.
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

                                    // Si rowsAffected es -1, se interpreta como ejecución exitosa (SP devuelve -1 cuando no reporta filas afectadas)
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
    }
}
