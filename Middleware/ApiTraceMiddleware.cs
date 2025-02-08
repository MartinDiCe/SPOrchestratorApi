using System.Diagnostics;
using System.Reactive.Linq;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ParameterServices;

namespace SPOrchestratorAPI.Middleware
{
    /// <summary>
    /// Middleware que captura y registra la traza final de las solicitudes y respuestas.
    /// Usa un ResponseCaptureStream para interceptar las escrituras en la respuesta y OnCompleted para capturar el contenido final,
    /// de forma que se registre el body completo y el código HTTP real, incluso en respuestas de error.
    /// </summary>
    public class ApiTraceMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            var parameterService = context.RequestServices.GetRequiredService<IParameterService>();
            
            var traceParam = await parameterService.GetByNameAsync("ApiTraceEnabled");
            if (traceParam == null || !traceParam.ParameterValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            
            context.Request.EnableBuffering();
            string requestBody;
            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
            
            string endpointIdentifier = context.Request.Path.ToString();
            
            string requestOrigin = context.Request.Headers.ContainsKey("X-Forwarded-For")
                ? context.Request.Headers["X-Forwarded-For"].ToString()
                : context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Guardar el stream original y reemplazarlo por un ResponseCaptureStream.
            var originalBodyStream = context.Response.Body;
            var captureStream = new ResponseCaptureStream(originalBodyStream);
            context.Response.Body = captureStream;

            context.Response.OnCompleted(() =>
            {
                stopwatch.Stop();
                string responseBodyText = captureStream.GetCapturedText();
                
                var trace = new ApiTrace
                {
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
                
                SPOrchestratorAPI.Traces.ApiTraceBus.TraceSubject.OnNext(trace);
                return Task.CompletedTask;
            });

            try
            {
                await next(context);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
