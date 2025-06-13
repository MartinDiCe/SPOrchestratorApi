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

                    var servicio = await _servicioService.GetByNameAsync(serviceName).FirstAsync();
                    if (servicio == null)
                        throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");

                    var configs = await _configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                    if (configs == null || configs.Count == 0)
                        throw new ResourceNotFoundException($"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");

                    var config = configs[0];
                    if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                        throw new InvalidOperationException("El nombre del endpoint no está definido en la configuración.");

                    _logger.LogInfo($"Configuración obtenida para el servicio '{serviceName}'. Endpoint: {config.NombreProcedimiento}");

                    string endpointUrl = config.NombreProcedimiento.Trim();

                    if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out _))
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

                    Dictionary<string, string> endpointConfig = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrWhiteSpace(config.JsonConfig))
                    {
                        try
                        {
                            endpointConfig = EndpointConfigHelper.ParseAndValidateConfig(config.JsonConfig)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
                        }
                        catch (FormatException ex)
                        {
                            throw new InvalidOperationException("Error en la configuración JSON del endpoint: " + ex.Message);
                        }
                    }

                    bool requiereApiKey = false;
                    if (endpointConfig.TryGetValue("RequiereApiKey", out var requiereApiKeyValue))
                    {
                        requiereApiKey = bool.TryParse(requiereApiKeyValue, out var r) && r;
                    }
                    string apiKey = "";
                    if (requiereApiKey)
                    {
                        if (!endpointConfig.TryGetValue("ApiKey", out apiKey!) || string.IsNullOrWhiteSpace(apiKey))
                        {
                            throw new InvalidOperationException("El endpoint requiere ApiKey, pero no se ha configurado correctamente en JsonConfig.");
                        }
                    }
                    
                    string tipoRequest = "POST";
                    if (endpointConfig.TryGetValue("TipoRequest", out var tipoRequestValue))
                    {
                        tipoRequest = tipoRequestValue.ToUpperInvariant();
                    }
                    _logger.LogInfo($"Tipo de request configurado: {tipoRequest}");

                    var convertedParams = ParameterConverter.ConvertParameters(parameters);
                    _logger.LogInfo($"Se procesaron {convertedParams.Count} parámetros para la llamada al endpoint.");

                    var request = new HttpRequestMessage(new HttpMethod(tipoRequest), endpointUrl);
                    
                    if (requiereApiKey)
                    {
                        // Sacamos AuthScheme (por defecto ApiKey)
                        var authScheme = endpointConfig.TryGetValue("AuthScheme", out var s)
                            ? s
                            : "ApiKey";

                        if (authScheme.Equals("ApiKey", StringComparison.OrdinalIgnoreCase))
                        {
                            // Legacy: header custom “ApiKey”
                            request.Headers.Add("ApiKey", apiKey);
                        }
                        else
                        {
                            // Cualquier otro esquema usa Authorization:<Scheme> <token>
                            request.Headers.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue(
                                    authScheme, 
                                    apiKey);
                        }
                    }

                    // 3) Logeamos manualmente los headers actuales
                    var headersString = string.Join("; ",
                        request.Headers
                            .Select(h => $"{h.Key}=[{string.Join(",", h.Value)}]"));

                    _logger.LogInfo($"Headers de la petición a {request.RequestUri}: {headersString}");
                    
                    if (tipoRequest == "GET")
                    {
                        var queryString = string.Join("&", convertedParams.Select(kvp =>
                            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString()!)}"));
                        var separator = endpointUrl.Contains("?") ? "&" : "?";
                        request.RequestUri = new Uri(endpointUrl + separator + queryString);
                    }
                    else if (tipoRequest == "DELETE")
                    {
                        string jsonContent = JsonSerializer.Serialize(convertedParams);
                        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        _logger.LogInfo($"Cuerpo de la solicitud: {jsonContent}");
                    }
                    else
                    {
                        string jsonContent = JsonSerializer.Serialize(convertedParams);
                        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        _logger.LogInfo($"Cuerpo de la solicitud: {jsonContent}");
                    }

                    _logger.LogInfo($"Llamando al endpoint: {request.RequestUri}");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInfo("Respuesta del endpoint recibida.");

                    return JsonSerializer.Deserialize<object>(responseBody)!;
                });
            });
        }
    }
}