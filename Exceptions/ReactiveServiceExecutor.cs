using System.Reactive.Linq;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Exceptions;

/// <summary>
/// Define un ejecutor de acciones reactivas que captura y maneja excepciones 
/// de manera unificada, sin acoplarse a la capa de presentación (HTTP).
/// </summary>
public class ReactiveServiceExecutor : IServiceExecutor
{
    private readonly ILoggerService<ReactiveServiceExecutor> _logger;

    /// <summary>
    /// Constructor de la clase <see cref="ReactiveServiceExecutor"/>.
    /// </summary>
    /// <param name="logger">
    /// Servicio de logging para registrar mensajes informativos o de error.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Se lanza si <paramref name="logger"/> es <c>null</c>.
    /// </exception>
    public ReactiveServiceExecutor(ILoggerService<ReactiveServiceExecutor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    /// <summary>
    /// Envuelve la ejecución de una acción de manera reactiva, capturando excepciones 
    /// específicas (<see cref="ServiceException"/>) y genéricas, registrando logs y 
    /// relanzando la excepción para que capas superiores puedan manejarla.
    /// </summary>
    /// <typeparam name="T">
    /// Tipo de dato que emitirá el observable devuelto por la acción.
    /// </typeparam>
    /// <param name="action">
    /// Func que retorna un <see cref="IObservable{T}"/> con la lógica a ejecutar de forma asíncrona o reactiva.
    /// </param>
    /// <returns>
    /// Un <see cref="IObservable{T}"/> que, al suscribirse, ejecutará la acción y 
    /// emitirá los elementos o la excepción resultante.
    /// </returns>
    public IObservable<T> ExecuteAsync<T>(Func<IObservable<T>> action)
    {
        return Observable
            .Defer(action)

            // Captura excepciones específicas de servicio
            .Catch<T, ServiceException>(ex =>
            {
                _logger.LogError($"Error específico de servicio: {ex.Message}", ex);
                // Relanzar la excepción para que la maneje el suscriptor
                return Observable.Throw<T>(ex);
            })

            // Captura excepciones genéricas
            .Catch<T, Exception>(ex =>
            {
                _logger.LogError($"Error genérico al ejecutar la acción: {ex.Message}", ex);
                return Observable.Throw<T>(ex);
            });
    }
}