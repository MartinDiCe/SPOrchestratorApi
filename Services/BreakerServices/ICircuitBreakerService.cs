namespace SPOrchestratorAPI.Services.BreakerServices;

public interface ICircuitBreakerService
{
    /// <summary>
    /// Indica si el circuito se encuentra abierto. Si está abierto y el período de enfriamiento se ha cumplido,
    /// se cierra el circuito para permitir nuevos intentos.
    /// </summary>
    bool IsOpen();
    
    /// <summary>
    /// Registra una falla. Si el número de fallos alcanza el umbral, abre el circuito.
    /// </summary>
    void RecordFailure();
    
    /// <summary>
    /// Registra un intento exitoso, reiniciando el contador de fallos y cerrando el circuito.
    /// </summary>
    void RecordSuccess();
}
