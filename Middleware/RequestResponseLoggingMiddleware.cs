using System.Text;

namespace SPOrchestratorAPI.Middleware;

/// <summary>
/// Middleware para loggear requests y responses de la aplicación.
/// </summary>
public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        // Log de la solicitud
        logger.LogInformation("📢 [REQUEST] {Method} {Path} | IP: {IpAddress}",
            context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);

        // Clonar el cuerpo de la solicitud para leerlo sin afectar la ejecución
        context.Request.EnableBuffering();
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                logger.LogInformation("🔹 Body: {Body}", body);
            }
        }

        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await next(context);

            // Log de la respuesta
            logger.LogInformation("📢 [RESPONSE] {StatusCode}", context.Response.StatusCode);

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}