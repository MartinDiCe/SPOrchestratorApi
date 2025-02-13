using System.Data;
using System.Reactive.Linq;
using Microsoft.Data.SqlClient;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;

namespace SPOrchestratorAPI.Services.VistasSqlServices;

public class VistaSqlService(
    IServicioConfiguracionService configService,
    ILoggerService<VistaSqlService> logger,
    IServiceExecutor serviceExecutor,
    IServicioService servicioService)
    : IVistaSqlService
{
    private readonly ILoggerService<VistaSqlService>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IServiceExecutor _serviceExecutor =
        serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));

    private readonly IServicioConfiguracionService _configService =
        configService ?? throw new ArgumentNullException(nameof(configService));

    private readonly IServicioService _servicioService =
        servicioService ?? throw new ArgumentNullException(nameof(servicioService));

    public IObservable<object> EjecutarVistaPorNombreAsync(string serviceName,
        IDictionary<string, object>? parameters = null)
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
                    throw new ResourceNotFoundException(
                        $"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");
                }

                var config = configs[0];
                if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                {
                    throw new InvalidOperationException(
                        "El nombre de la vista SQL no está definido en la configuración.");
                }

                _logger.LogInfo(
                    $"Configuración obtenida para el servicio '{serviceName}'. Vista: {config.NombreProcedimiento}");
                
                object resultData;
                using (var connection = new SqlConnection(config.ConexionBaseDatos))
                {
                    await connection.OpenAsync();
                    _logger.LogInfo($"Conexión abierta a la base de datos: {config.ConexionBaseDatos}");
                    
                    string query = $"SELECT * FROM {config.NombreProcedimiento}";
                    using (var command = new SqlCommand(query, connection)
                           {
                               CommandType = CommandType.Text
                           })
                    {
                        if (parameters != null && parameters.Count > 0)
                        {
                            foreach (var kvp in parameters)
                            {
                                command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                            }
                        }

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

                _logger.LogInfo("La consulta a la vista se ejecutó correctamente y se obtuvo la respuesta.");
                return resultData;
            });
        });
    }
}