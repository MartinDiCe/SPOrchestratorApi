using System.Text;

namespace SPOrchestratorAPI.Middleware
{
    /// <summary>
    /// Middleware que registra (loguea) la información de cada request y response,
    /// incluyendo método HTTP, ruta y contenido del cuerpo, así como el código de estado de la respuesta.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        /// <summary>
        /// Constructor que inyecta el siguiente middleware y el servicio de logging.
        /// </summary>
        /// <param name="next">Delegate que representa el siguiente middleware.</param>
        /// <param name="logger">Servicio para registrar mensajes de log.</param>
        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Método principal del middleware. Registra la petición y la respuesta,
        /// incluyendo la lectura del cuerpo de la request y el código de estado de la response.
        /// </summary>
        /// <param name="context">Contexto HTTP con información de la solicitud y respuesta.</param>
        public async Task Invoke(HttpContext context)
        {
            // [1] LOG DE LA REQUEST
            _logger.LogInformation("📢 [REQUEST] {Method} {Path} | IP: {IpAddress}",
                context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);

            // Leer body de la request
            context.Request.EnableBuffering();
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    _logger.LogInformation("🔹 Body: {Body}", body);
                }
            }

            // [2] INTERCEPTAR RESPUESTA
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // [3] Invocar al siguiente middleware (incluido tu ExceptionMiddleware si está después)
            await _next(context);

            // [4] Log del código de estado
            _logger.LogInformation("📢 [RESPONSE] {StatusCode}", context.Response.StatusCode);

            // [5] Mover posición a 0 para leer el contenido devuelto
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            // Opcional: loguear el body completo (cuidado en producción si es grande)
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                _logger.LogInformation("🔸 [RESPONSE BODY]: {Body}", responseText);
            }

            // [6] Copiar el contenido de la respuesta interceptada al flujo original
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}
