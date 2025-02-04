using Microsoft.AspNetCore.Mvc;

namespace SPOrchestratorAPI.Exceptions
{
    /// <summary>
    /// Interfaz que define un servicio para ejecutar acciones de manera reactiva y manejar errores.
    /// </summary>
    public interface IServiceExecutor
    {
        /// <summary>
        /// Ejecuta una acción de servicio de manera reactiva y maneja errores.
        /// </summary>
        /// <param name="action">La acción a ejecutar de manera reactiva.</param>
        /// <returns>Un observable que emite un resultado de acción.</returns>
        IObservable<IActionResult> ExecuteAsync(Func<IObservable<IActionResult>> action);
    }
}