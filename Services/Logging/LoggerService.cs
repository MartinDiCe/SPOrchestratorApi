namespace SPOrchestratorAPI.Services.Logging
{
    /// <summary>
    /// Implementación de <see cref="ILoggerService{T}"/> que envuelve un <see cref="ILogger{T}"/> de ASP.NET Core.
    /// </summary>
    /// <typeparam name="T">
    /// Clase o tipo que se utilizará como categoría de logging. 
    /// Usualmente, se pasa el propio tipo de la clase que hace uso del logger.
    /// </typeparam>
    public class LoggerService<T> : ILoggerService<T>
    {
        private readonly ILogger<T> _logger;

        /// <summary>
        /// Constructor de la clase <see cref="LoggerService{T}"/>.
        /// </summary>
        /// <param name="logger">Instancia de <see cref="ILogger{T}"/> inyectada por dependencias.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="logger"/> es nulo.</exception>
        public LoggerService(ILogger<T> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void LogInfo(string message)
        {
            _logger.LogInformation("ℹ️ " + message);
        }

        /// <inheritdoc />
        public void LogWarning(string message)
        {
            _logger.LogWarning("⚠️ " + message);
        }

        /// <inheritdoc />
        public void LogError(string message, Exception? ex = null)
        {
            // Al llamar a _logger.LogError, si ex no es nulo se incluye el stack trace.
            // El primer parámetro (ex) define la excepción que se va a loguear.
            _logger.LogError(ex, "❌ " + message);
        }
    }
}