namespace SPOrchestratorAPI.Services.Logging;

/// <summary>
/// Interfaz genérica para Logging.
/// </summary>
public interface ILoggerService<T>
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
}