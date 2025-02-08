using SPOrchestratorAPI.Services.Breaker;

namespace SPOrchestratorAPI.Services.RetriesService
{
    /// <inheritdoc />
    public class RetryPolicy : IRetryPolicy
    {
        private readonly ICircuitBreaker _circuitBreaker;
        private readonly ILogger<RetryPolicy> _logger;

        public RetryPolicy(ICircuitBreaker circuitBreaker, ILogger<RetryPolicy> logger)
        {
            _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, int perAttemptTimeoutMs, int maxGlobalTimeoutMs, int maxAttempts)
        {
            DateTime startTime = DateTime.UtcNow;
            int attempts = 0;

            _logger.LogInformation("Inicio de ejecución con RetryPolicy. Timeout por intento: {PerAttemptTimeoutMs} ms, Timeout global: {MaxGlobalTimeoutMs} ms, Máximo de intentos: {MaxAttempts}.",
                perAttemptTimeoutMs, maxGlobalTimeoutMs, maxAttempts);

            if (_circuitBreaker.IsOpen())
            {
                _logger.LogWarning("Circuit breaker abierto. No se permite la ejecución.");
                throw new Exception("El sistema está en mantenimiento temporal, intente más tarde.");
            }

            while (attempts < maxAttempts)
            {
                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (elapsedMs > maxGlobalTimeoutMs)
                {
                    _logger.LogError("Tiempo global transcurrido {ElapsedMs} ms excede el límite global {MaxGlobalTimeoutMs} ms.", elapsedMs, maxGlobalTimeoutMs);
                    throw new TimeoutException("El tiempo de la solicitud excede el límite global permitido.");
                }

                using (var cts = new CancellationTokenSource(perAttemptTimeoutMs))
                {
                    try
                    {
                        _logger.LogInformation("Intento {Attempt} iniciado.", attempts + 1);
                        T result = await action(cts.Token);
                        _logger.LogInformation("Intento {Attempt} exitoso.", attempts + 1);
                        _circuitBreaker.RecordSuccess();
                        return result;
                    }
                    catch (Exception ex) when (IsTransientError(ex))
                    {
                        _logger.LogWarning(ex, "Error transitorio en el intento {Attempt}.", attempts + 1);
                        _circuitBreaker.RecordFailure();
                        if (_circuitBreaker.IsOpen())
                        {
                            _logger.LogError("Circuit breaker se abrió después de {Attempt} intentos.", attempts + 1);
                            throw new Exception("El sistema está en mantenimiento temporal, intente más tarde.");
                        }
                        int delayMs = ComputeBackoffDelay(attempts);
                        _logger.LogWarning("Reintentando en {Delay} ms...", delayMs);
                        await Task.Delay(delayMs);
                        attempts++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error no transitorio detectado en el intento {Attempt}. Abortando.", attempts + 1);
                        throw;
                    }
                }
            }

            _logger.LogError("Se alcanzó el número máximo de reintentos ({MaxAttempts}) sin éxito.", maxAttempts);
            throw new Exception("Se alcanzó el número máximo de reintentos sin éxito.");
        }

        /// <summary>
        /// Determina si el error es transitorio (por ejemplo, timeout o deadlock).
        /// Aquí se puede extender la lógica para otros tipos de error transitorios.
        /// </summary>
        private bool IsTransientError(Exception ex)
        {
            return ex is TimeoutException;
        }

        /// <summary>
        /// Calcula el tiempo de espera (backoff) para el próximo intento utilizando un esquema exponencial.
        /// </summary>
        private int ComputeBackoffDelay(int attempt)
        {
            int baseDelayMs = 500;
            int delay = baseDelayMs * (int)Math.Pow(2, attempt);
            _logger.LogDebug("Backoff delay calculado para el intento {Attempt}: {Delay} ms.", attempt + 1, delay);
            return delay;
        }
    }
}
