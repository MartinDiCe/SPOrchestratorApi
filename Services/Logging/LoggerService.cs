namespace SPOrchestratorAPI.Services.Logging;

/// <summary>
/// Implementación genérica de Logging.
/// </summary>
public class LoggerService<T> : ILoggerService<T>
{
    private readonly ILogger<T> _logger;

    public LoggerService(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation("ℹ️ " + message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning("⚠️ " + message);
    }

    public void LogError(string message, Exception? ex = null)
    {
        _logger.LogError(ex, "❌ " + message);
    }
}