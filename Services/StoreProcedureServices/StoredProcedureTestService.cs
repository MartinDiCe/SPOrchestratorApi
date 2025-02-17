using System.Reactive.Linq;
using System.Text.Json;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;

namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    /// <summary>
    /// Servicio para la ejecución de stored procedures que no retornan datos, utilizando la configuración
    /// definida en <see cref="ServicioConfiguracion"/>. Aplica un enfoque reactivo y delega la ejecución al executor
    /// correspondiente según el proveedor.
    /// </summary>
    public class StoredProcedureTestService(
        IServicioConfiguracionService configService,
        ILoggerService<StoredProcedureTestService> logger,
        IServiceExecutor serviceExecutor,
        IStoredProcedureExecutorFactory executorFactory)
        : IStoredProcedureTestService
    {
        private readonly ILoggerService<StoredProcedureTestService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceExecutor _serviceExecutor = serviceExecutor ?? throw new ArgumentNullException(nameof(serviceExecutor));
        private readonly IServicioConfiguracionService _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        private readonly IStoredProcedureExecutorFactory _executorFactory = executorFactory ?? throw new ArgumentNullException(nameof(executorFactory));

        /// <inheritdoc />
        public IObservable<int> EjecutarSpAsync(int idConfiguracion, IDictionary<string, object>? parameters = null)
        {
            return _serviceExecutor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Iniciando ejecución del SP para la configuración con ID {idConfiguracion}.");

                    var config = await _configService.GetByIdAsync(idConfiguracion).FirstAsync();
                    if (config == null)
                    {
                        throw new ResourceNotFoundException($"No se encontró configuración con ID {idConfiguracion}.");
                    }
                    if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                    {
                        throw new InvalidOperationException("El nombre del stored procedure no está definido en la configuración.");
                    }
                    _logger.LogInfo($"Configuración obtenida. Proveedor: {config.Provider}, SP: {config.NombreProcedimiento}");

                    // Validar parámetros según la configuración
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
                        var expectedValues = new HashSet<string>(expectedParams.Values, StringComparer.OrdinalIgnoreCase);
                        var missingParams = new List<string>();
                        var extraParams = new List<string>();

                        var parametersCI = parameters != null
                            ? new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase)
                            : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                        foreach (var expected in expectedValues)
                        {
                            if (!parametersCI.ContainsKey(expected))
                            {
                                missingParams.Add(expected);
                            }
                        }
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

                    var executor = _executorFactory.GetExecutor(config.Provider);
                    int rowsAffected = await executor.ExecuteNonQueryAsync(config, parameters);

                    _logger.LogInfo($"Ejecución del SP '{config.NombreProcedimiento}' completada. Filas afectadas: {rowsAffected}.");
                    return rowsAffected;
                });
            });
        }
    }
}
