using Microsoft.AspNetCore.Mvc;
using SPOrchestratorAPI.Services.Logging;
using System.Reactive.Linq;

namespace SPOrchestratorAPI.Exceptions
{
    /// <summary>
    /// Implementación de IServiceExecutor para ejecutar acciones y manejar errores de manera reactiva.
    /// </summary>
    public class ReactiveServiceExecutor : IServiceExecutor
    {
        private readonly ILoggerService<ReactiveServiceExecutor> _logger;

        /// <summary>
        /// Constructor que recibe el logger para registrar errores.
        /// </summary>
        /// <param name="logger">El servicio de logging que se usará para registrar errores.</param>
        public ReactiveServiceExecutor(ILoggerService<ReactiveServiceExecutor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Ejecuta una acción de servicio de manera reactiva y maneja errores.
        /// </summary>
        /// <param name="action">La acción que se ejecutará de manera reactiva.</param>
        /// <returns>Un observable que emite un resultado de acción.</returns>
        public IObservable<IActionResult> ExecuteAsync(Func<IObservable<IActionResult>> action)
        {
            // Ejecutamos la acción de manera reactiva y manejamos los errores.
            return Observable.Defer(action)
                .Catch<IActionResult, Exception>(ex =>
                {
                    // Asegúrate de que el logger esté capturando el mensaje y la excepción
                    _logger.LogError($"Error ejecutando la acción: {ex.Message}", ex);

                    // Devolvemos un error 500
                    var errorResponse = new ObjectResult(new { mensaje = ex.Message })
                    {
                        StatusCode = 500
                    };
                    return Observable.Return(errorResponse);
                })
                .Catch<IActionResult, ServiceException>(ex =>
                {
                    // Asegúrate de que el logger esté capturando el mensaje específico para ServiceException
                    _logger.LogError($"Error específico de servicio: {ex.Message}", ex);

                    var errorResponse = new ObjectResult(new { mensaje = ex.Message, errorCode = ex.ErrorCode })
                    {
                        StatusCode = 500
                    };
                    return Observable.Return(errorResponse);
                });;
        }
    }
}
