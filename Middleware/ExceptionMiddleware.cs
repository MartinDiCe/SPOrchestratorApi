using System.Net;
using System.Text.Json;
using SPOrchestratorAPI.Exceptions;

namespace SPOrchestratorAPI.Middleware
{
    /// <summary>
    /// Middleware que captura excepciones no controladas en la aplicación,
    /// registra logs y retorna una respuesta HTTP adecuada (404, 400, 500, etc.).
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        /// Constructor que inyecta el siguiente middleware en la tubería y el servicio de logging.
        /// </summary>
        /// <param name="next">Delegate que representa el siguiente middleware.</param>
        /// <param name="logger">Servicio para registrar mensajes de log.</param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Método principal de este middleware. Envuelve la ejecución
        /// del siguiente middleware en un bloque try/catch para capturar excepciones.
        /// </summary>
        /// <param name="context">
        /// Contexto HTTP que contiene información de la solicitud y respuesta.
        /// </param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Continúa con el siguiente middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // Registramos el error y construimos la respuesta adecuada
                _logger.LogError(ex, "Ocurrió un error no controlado.");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Construye la respuesta HTTP cuando ocurre una excepción.
        /// Retorna JSON con la información del error y asigna el código de estado apropiado.
        /// </summary>
        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var response = new
            {
                mensaje = ex.Message,
                detalle = ex.InnerException?.Message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}