using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Services.EndpointServices
{
    /// <summary>
    /// Servicio para la ejecución de llamadas a endpoints, utilizando HttpClient.
    /// Esta implementación sigue el enfoque reactivo (enfoque B) y la misma estructura que el resto de servicios.
    /// </summary>
    public class EndpointService(
        ILoggerService<EndpointService> logger,
        HttpClient httpClient)
        : IEndpointService
    {
        private readonly ILoggerService<EndpointService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        /// <inheritdoc />
        public IObservable<object> EjecutarEndpointPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null)
        {
            return Observable.FromAsync(async () =>
            {

                string endpointUrl = $"https://api.tuendpoint.com/{serviceName}";
                
                string jsonContent = parameters != null ? JsonSerializer.Serialize(parameters) : string.Empty;
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInfo($"Llamando al endpoint: {endpointUrl} con parámetros: {jsonContent}");

                // Realiza la llamada HTTP (ejemplo con POST)
                var response = await _httpClient.PostAsync(endpointUrl, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInfo("Respuesta del endpoint recibida.");
                
                return JsonSerializer.Deserialize<object>(responseBody)!;
            });
        }
    }
}
