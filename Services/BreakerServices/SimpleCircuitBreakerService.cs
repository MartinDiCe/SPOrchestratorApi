namespace SPOrchestratorAPI.Services.BreakerServices
{
    /// <summary>
    /// Implementa la lógica de un Circuit Breaker simple para controlar el flujo de ejecución en caso de múltiples fallos.
    /// Registra fallos y, si se supera el umbral configurado, abre el circuito por un tiempo determinado.
    /// </summary>
    public class SimpleCircuitBreakerService : ICircuitBreakerService
    {
        private readonly int _failureThreshold;
        private readonly TimeSpan _openDuration;
        private int _failureCount = 0;
        private DateTime _lastFailureTime = DateTime.MinValue;
        private bool _isOpen = false;
        private readonly ILogger<SimpleCircuitBreakerService> _logger;

        /// <summary>
        /// Crea una instancia de <see cref="SimpleCircuitBreakerService"/>.
        /// </summary>
        /// <param name="failureThreshold">Número de fallos consecutivos permitidos antes de abrir el circuito.</param>
        /// <param name="openDuration">Tiempo durante el cual el circuito permanecerá abierto antes de intentar cerrarlo.</param>
        /// <param name="logger">Logger para registrar información de la operación del Circuit Breaker.</param>
        public SimpleCircuitBreakerService(int failureThreshold, TimeSpan openDuration, ILogger<SimpleCircuitBreakerService> logger)
        {
            _failureThreshold = failureThreshold;
            _openDuration = openDuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public bool IsOpen()
        {
            if (_isOpen && (DateTime.UtcNow - _lastFailureTime) > _openDuration)
            {
                _logger.LogInformation("El tiempo de enfriamiento del Circuit Breaker ha finalizado. Cerrando el circuito.");
                _isOpen = false;
                _failureCount = 0; // Reiniciamos el contador de fallos.
            }
            
            _logger.LogDebug("Estado del Circuit Breaker (IsOpen): {IsOpen}. Fallos actuales: {FailureCount}.", _isOpen, _failureCount);
            return _isOpen;
        }

        /// <inheritdoc />
        public void RecordFailure()
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            _logger.LogWarning("Registro de fallo. Conteo actual de fallos: {FailureCount}.", _failureCount);

            if (_failureCount >= _failureThreshold)
            {
                _isOpen = true;
                _logger.LogError("Se alcanzó el umbral de fallos ({FailureThreshold}). Circuit Breaker ABIERTO.", _failureThreshold);
            }
        }

        /// <inheritdoc />
        public void RecordSuccess()
        {
            _logger.LogInformation("Se registró un éxito, reiniciando el contador de fallos y cerrando el Circuit Breaker.");
            _failureCount = 0;
            _isOpen = false;
        }
    }
}
