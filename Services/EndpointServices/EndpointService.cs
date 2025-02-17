using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;

namespace SPOrchestratorAPI.Services.EndpointServices
{
    /// <summary>
    /// Servicio para la ejecución de llamadas a endpoints utilizando HttpClient.
    /// Este servicio sigue el mismo enfoque reactivo y patrón que VistaSqlService, obteniendo la configuración
    /// a partir del nombre del servicio y usando el executor para la ejecución centralizada.
    /// </summary>
    public class EndpointService(
        ILoggerService<EndpointService> logger,
        IServiceExecutor executor,
        IServicioService servicioService,
        IServicioConfiguracionService configService,
        HttpClient httpClient)
        : IEndpointService
    {
        private readonly ILoggerService<EndpointService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        private readonly IServicioService _servicioService = servicioService ?? throw new ArgumentNullException(nameof(servicioService));
        private readonly IServicioConfiguracionService _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        /// <inheritdoc />
        public IObservable<object> EjecutarEndpointPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null)
        {
            return _executor.ExecuteAsync(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    _logger.LogInfo($"Iniciando ejecución de endpoint para el servicio '{serviceName}'.");

                    // Obtener el servicio por su nombre
                    var servicio = await _servicioService.GetByNameAsync(serviceName).FirstAsync();
                    if (servicio == null)
                        throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");

                    // Obtener la configuración asociada al servicio
                    var configs = await _configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                    if (configs == null || configs.Count == 0)
                        throw new ResourceNotFoundException($"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");

                    var config = configs[0];
                    if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                        throw new InvalidOperationException("El nombre del endpoint no está definido en la configuración.");

                    _logger.LogInfo($"Configuración obtenida para el servicio '{serviceName}'. Endpoint: {config.NombreProcedimiento}");

                    // Se usa el valor almacenado en la configuración como URL del endpoint
                    string endpointUrl = config.NombreProcedimiento.Trim();

                    if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out var uriResult))
                    {
                        if (_httpClient.BaseAddress != null)
                        {
                            endpointUrl = new Uri(_httpClient.BaseAddress, endpointUrl).ToString();
                        }
                        else
                        {
                            throw new InvalidOperationException("La URL del endpoint no es absoluta y no se ha definido BaseAddress en HttpClient.");
                        }
                    }

                    _logger.LogInfo($"URL final del endpoint: {endpointUrl}");

                    // Convertir parámetros a tipos nativos utilizando el helper ParameterConverter
                    var convertedParams = ParameterConverter.ConvertParameters(parameters);
                    _logger.LogInfo($"Se procesaron {convertedParams.Count} parámetros para la llamada al endpoint.");

                    string jsonContent = JsonSerializer.Serialize(convertedParams);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    _logger.LogInfo($"Llamando al endpoint: {endpointUrl} con parámetros: {jsonContent}");

                    var response = await _httpClient.PostAsync(endpointUrl, content);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInfo("Respuesta del endpoint recibida.");

                    return JsonSerializer.Deserialize<object>(responseBody)!;
                });
            });
        }
    }
}