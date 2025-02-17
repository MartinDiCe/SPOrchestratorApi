using System.Data;
using System.Reactive.Linq;
using Microsoft.Data.SqlClient;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;
using SPOrchestratorAPI.Helpers;

namespace SPOrchestratorAPI.Services.VistasSqlServices
{
    /// <summary>
    /// Servicio para la ejecución de consultas a vistas SQL utilizando la configuración
    /// definida en <see cref="ServicioConfiguracion"/>.
    /// </summary>
    public class VistaSqlService(
        IServicioConfiguracionService configService,
        ILoggerService<VistaSqlService> logger,
        IServiceExecutor serviceExecutor,
        IServicioService servicioService)
        : IVistaSqlService
    {
        private readonly IServicioConfiguracionService _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        private readonly ILoggerService<VistaSqlService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceExecutor _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
        private readonly IServicioService _servicioService = servicioService ?? throw new ArgumentNullException(nameof(servicioService));

        public IObservable<object> EjecutarVistaPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Iniciando lectura de la vista para el servicio '{serviceName}'.");

                    // Obtener el servicio por su nombre
                    var servicio = await _servicioService.GetByNameAsync(serviceName).FirstAsync();
                    if (servicio == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");
                    }
                    var configs = await _configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                    if (configs == null || configs.Count == 0)
                    {
                        throw new ResourceNotFoundException($"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");
                    }
                    var config = configs[0];
                    if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                    {
                        throw new InvalidOperationException("El nombre de la vista SQL no está definido en la configuración.");
                    }
                    _logger.LogInfo($"Configuración obtenida para el servicio '{serviceName}'. Vista: {config.NombreProcedimiento}");

                    // Convertir parámetros a tipos nativos
                    var convertedParams = ParameterConverter.ConvertParameters(parameters);
                    _logger.LogInfo($"Se procesaron {convertedParams.Count} parámetros para la consulta de la vista.");

                    // Construir la consulta completa usando la cláusula WHERE si hay filtros
                    string query = VistaSqlQueryBuilder.BuildQuery(config.NombreProcedimiento, convertedParams);
                    _logger.LogInfo($"Consulta construida: {query}");

                    object resultData;
                    using (var connection = new SqlConnection(config.ConexionBaseDatos))
                    {
                        await connection.OpenAsync();
                        _logger.LogInfo($"Conexión abierta a la base de datos: {config.ConexionBaseDatos}");
                        using (var command = new SqlCommand(query, connection)
                        {
                            CommandType = CommandType.Text
                        })
                        {
                            // Agregar los parámetros convertidos al comando, si se usan en la consulta
                            foreach (var kvp in convertedParams)
                            {
                                if (query.Contains($"@{kvp.Key}"))
                                {
                                    command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                                }
                            }
                            _logger.LogInfo($"Se agregaron {command.Parameters.Count} parámetros a la consulta de la vista.");
                            
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
                                // Si la consulta no retorna registros, retornar un mensaje informativo
                                if (resultList.Count == 0)
                                {
                                    _logger.LogInfo("La consulta a la vista no devolvió registros.");
                                    resultData = new { message = "No se encontraron resultados con los parámetros especificados." };
                                }
                                else
                                {
                                    resultData = resultList;
                                }
                            }
                        }
                    }
                    _logger.LogInfo("La consulta a la vista se ejecutó correctamente y se obtuvo la respuesta.");
                    return resultData;
                });
            });
        }
    }
}