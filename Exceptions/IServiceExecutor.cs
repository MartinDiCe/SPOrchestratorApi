namespace SPOrchestratorAPI.Exceptions
{
    /// <summary>
    /// Interfaz que define un servicio para ejecutar acciones de manera reactiva y manejar errores.
    /// </summary>
    public interface IServiceExecutor
    {
        /// <summary>
        /// Ejecuta una acción de manera reactiva y maneja errores, retornando un observable de tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo de dato que emitirá el observable.</typeparam>
        /// <param name="action">Func que retorna un IObservable de tipo <typeparamref name="T"/>.</param>
        /// <returns>Un observable que emite objetos de tipo <typeparamref name="T"/> o lanza excepciones.</returns>
        IObservable<T> ExecuteAsync<T>(Func<IObservable<T>> action);
    }
}