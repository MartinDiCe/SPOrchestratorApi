namespace SPOrchestratorAPI.Services.Logging
{
    /// <summary>
    /// Define métodos de logging genérico para la clase o tipo <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// Clase o tipo que se utilizará como categoría de logging. 
    /// Usualmente se pasa el propio tipo de la clase que hace uso del logger.
    /// </typeparam>
    public interface ILoggerService<T>
    {
        /// <summary>
        /// Registra un mensaje informativo en el log.
        /// </summary>
        /// <param name="message">El mensaje a registrar.</param>
        void LogInfo(string message);

        /// <summary>
        /// Registra un mensaje de advertencia en el log.
        /// </summary>
        /// <param name="message">El mensaje a registrar.</param>
        void LogWarning(string message);

        /// <summary>
        /// Registra un mensaje de error en el log, con la opción de incluir una excepción.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del error.</param>
        /// <param name="ex">Excepción asociada (opcional).</param>
        void LogError(string message, Exception? ex = null);
    }
}