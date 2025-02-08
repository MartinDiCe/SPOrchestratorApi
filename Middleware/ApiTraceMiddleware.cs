using System.Diagnostics;
using System.Reactive.Linq;
using System.Text.Json;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ApiTraceServices;
using SPOrchestratorAPI.Services.ParameterServices;

namespace SPOrchestratorAPI.Middleware
{
    /// <summary>
    /// Middleware que captura y registra la traza de las solicitudes y respuestas de la API.
    /// La traza incluye información como el tiempo de ejecución, payloads, origen de la solicitud,
    /// nombre del endpoint (ServiceName) y, opcionalmente, el nombre del servicio extraído del JSON de la solicitud (ServiceId).
    /// La traza se registra únicamente si el parámetro "ApiTraceEnabled" está habilitado.
    /// </summary>
    public class ApiTraceMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApiTraceMiddleware"/>.
        /// </summary>
        /// <param name="next">El siguiente middleware en el pipeline.</param>
        public ApiTraceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoca el middleware para capturar y registrar la traza de la solicitud y respuesta.
        /// </summary>
        /// <param name="context">El contexto HTTP actual.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task Invoke(HttpContext context)
        {
            var parameterService = context.RequestServices.GetRequiredService<IParameterService>();
            var apiTraceService = context.RequestServices.GetRequiredService<IApiTraceService>();
            
            var traceParam = await parameterService.GetByNameAsync("ApiTraceEnabled");
            if (traceParam != null && traceParam.ParameterValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var stopwatch = Stopwatch.StartNew();

                context.Request.EnableBuffering();
                string requestBody;
                using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                string serviceIdValue = "";
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(requestBody))
                    {
                        if (doc.RootElement.TryGetProperty("serviceName", out JsonElement serviceNameElement))
                        {
                            serviceIdValue = serviceNameElement.GetString() ?? "";
                        }
                    }
                }
                catch (Exception)
                {
                    // Si falla el parseo (por ejemplo, el request no es JSON), se deja serviceIdValue vacío.
                }

                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);
                stopwatch.Stop();

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                string endpointIdentifier = context.Request.Path.ToString();

                string requestOrigin = context.Request.Headers.ContainsKey("X-Forwarded-For")
                    ? context.Request.Headers["X-Forwarded-For"].ToString()
                    : context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var trace = new ApiTrace
                {
                    ServiceId = serviceIdValue,
                    ServiceName = endpointIdentifier,
                    RequestOrigin = requestOrigin,
                    RequestTimestamp = DateTime.UtcNow,
                    HttpMethod = context.Request.Method,
                    RequestPayload = requestBody,
                    ResponsePayload = responseBodyText,
                    ExecutionTimeSeconds = stopwatch.Elapsed.TotalSeconds,
                    PayloadSize = responseBodyText.Length,
                    HttpResponseCode = context.Response.StatusCode
                };

                await apiTraceService.CreateAsync(trace);

                await responseBody.CopyToAsync(originalBodyStream);
            }
            else
            {
                await _next(context);
            }
        }
    }
}